using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.Model;
using FlorianAlbert.FinanceObserver.Server.CrossCutting.Infrastructure;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore;

public class Repository<TKey, TEntity> : IRepository<TKey, TEntity>
    where TEntity : BaseEntity<TKey>
    where TKey : IParsable<TKey>,
    IEquatable<TKey>
{
    private readonly DbContext _context;
    private readonly InclusionEvaluator _inclusionEvaluator;

    private DbSet<TEntity>? _set;

    internal Repository(DbContext dbContext,
        InclusionEvaluator inclusionEvaluator)
    {
        _context = dbContext;
        _inclusionEvaluator = inclusionEvaluator;
    }

    private DbSet<TEntity> _Set => _set ??= _context.Set<TEntity>();

    public Task<IQueryable<TEntity>> QueryAsync(Inclusion<TKey, TEntity>[]? includes = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> queryable = InclusionEvaluator.Evaluate(_Set.AsNoTracking(), includes?.ToImmutableArray() ?? [], cancellationToken);

        return Task.FromResult(queryable);
    }

    public async Task<Result<TEntity>> FindAsync(TKey id, Inclusion<TKey, TEntity>[]? includes = null, CancellationToken cancellationToken = default)
    {
        TEntity? entity = (await QueryAsync(includes, cancellationToken)).SingleOrDefault(entity => entity.Id.Equals(id));

        if (entity is null)
        {
            return Result.Failure<TEntity>(Errors.EntityNotFoundError);
        }

        return Result.Success(entity);
    }

    public Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return ExistsAsync(e => e.Id.Equals(id), cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> condition, Inclusion<TKey, TEntity>[]? includes = null,
        CancellationToken cancellationToken = default)
    {
        return await (await QueryAsync(includes, cancellationToken)).AnyAsync(condition, cancellationToken);
    }

    public async Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DateTimeOffset createdTime = DateTimeOffset.UtcNow;

        EntityEntry<TEntity> newEntry = _context.Attach(entity);
        newEntry.State = EntityState.Added;

        foreach (EntityEntry? entry in _context.ChangeTracker.Entries().Where(entry => entry.State == EntityState.Added))
        {
            ((LifecycleTrackable) entry.Entity).CreatedDate = createdTime;
            ((LifecycleTrackable) entry.Entity).UpdatedDate = createdTime;
        }

        foreach (EntityEntry? entry in _context.ChangeTracker.Entries().Where(entry => entry.State == EntityState.Modified))
        {
            ((LifecycleTrackable) entry.Entity).UpdatedDate = createdTime;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _context.ChangeTracker.Clear();

        return newEntry.Entity;
    }

    public async Task InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        TEntity[] entitiesArray = entities as TEntity[] ?? entities.ToArray();

        DateTimeOffset createdTime = DateTimeOffset.UtcNow;
        foreach (TEntity entity in entitiesArray)
        {
            EntityEntry<TEntity> entry = _context.Attach(entity);
            entry.State = EntityState.Added;
        }

        foreach (EntityEntry? entry in _context.ChangeTracker.Entries().Where(entry => entry.State == EntityState.Added))
        {
            ((LifecycleTrackable) entry.Entity).CreatedDate = createdTime;
            ((LifecycleTrackable) entry.Entity).UpdatedDate = createdTime;
        }

        foreach (EntityEntry? entry in _context.ChangeTracker.Entries().Where(entry => entry.State == EntityState.Modified))
        {
            ((LifecycleTrackable) entry.Entity).UpdatedDate = createdTime;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _context.ChangeTracker.Clear();
    }

    public async Task DeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        if (entities is IQueryable<TEntity> queryable)
        {
            await queryable.ExecuteDeleteAsync(cancellationToken: cancellationToken);

            return;
        }

        object[] entitiesArray = entities as object[] ?? entities.ToArray();

        _context.AttachRange(entitiesArray);

        _context.RemoveRange(entitiesArray);

        await _context.SaveChangesAsync(cancellationToken);

        _context.ChangeTracker.Clear();
    }

    public async Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, Inclusion<TKey, TEntity>[]? includes = null,
        CancellationToken cancellationToken = default)
    {
        await DeleteAsync((await QueryAsync(includes, cancellationToken)).Where(predicate), cancellationToken);
    }

    public Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return DeleteAsync(e => e.Id.Equals(id), cancellationToken: cancellationToken);
    }

    public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return DeleteAsync(entity.Id, cancellationToken);
    }

    public Task UpdateAsync(TEntity entity, Update<TEntity>[] updates, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(e => e.Id.Equals(entity.Id), updates, cancellationToken);
    }

    public async Task<int> UpdateAsync(Expression<Func<TEntity, bool>> predicate, Update<TEntity>[] updates,
        CancellationToken cancellationToken = default)
    {
        var validUpdates = updates.Where(u => u.SelectorExpression.Body is not MemberExpression
        {
            Expression: ParameterExpression,
            Member:
            {
                MemberType: MemberTypes.Property,
                Name: nameof(BaseEntity<TKey>.UpdatedDate) or nameof(BaseEntity<TKey>.CreatedDate)
                or nameof(BaseEntity<TKey>.Id)
            }
        }).ToList();

        validUpdates.Add(Update<TEntity>.With(e => e.UpdatedDate, DateTimeOffset.UtcNow));

        MethodInfo[] methods = typeof(SetPropertyCalls<TEntity>)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public);

        MethodInfo m = methods.Single(methodInfo =>
            methodInfo is { Name: nameof(SetPropertyCalls<object>.SetProperty), IsGenericMethod: true } &&
            methodInfo.GetParameters() is { Length: 2 } parameters
            && parameters.All(parameterInfo => parameterInfo.ParameterType is { IsGenericType: true } parameterType &&
                                               parameterType.GetGenericTypeDefinition() == typeof(Func<,>)));

        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>>
            baseExpression = setPropertyCall => setPropertyCall;

        var updateExpression = Expression.Lambda<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>>(validUpdates.Aggregate(
                baseExpression.Body,
                (currentExpr, nextUpdate) =>
                    Expression.Call(currentExpr,
                        m.MakeGenericMethod(nextUpdate.SelectorExpression.ReturnType),
                        nextUpdate.SelectorExpression,
                        nextUpdate.ValueExpression)),
            baseExpression.Parameters);

        IQueryable<TEntity> entitiesToUpdate = _Set.Where(predicate);

        int updatedRowsCount = await entitiesToUpdate.ExecuteUpdateAsync(updateExpression, cancellationToken: cancellationToken);

        return updatedRowsCount;
    }
}