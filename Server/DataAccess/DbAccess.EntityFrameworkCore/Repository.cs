using System.Linq.Expressions;
using System.Reflection;
using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.InfrastructureTypes;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Data;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore;

public class Repository<TKey, TEntity> : IRepository<TKey, TEntity>
    where TEntity : BaseEntity<TKey>
    where TKey : IParsable<TKey>,
    IEquatable<TKey>
{
    private readonly FinanceObserverContext _context;
    private readonly IInclusionEvaluator _includableEvaluator;

    private DbSet<TEntity>? _set;

    internal Repository(FinanceObserverContext dbContext,
        IInclusionEvaluator includableEvaluator)
    {
        _context = dbContext;
        _includableEvaluator = includableEvaluator;
    }

    private DbSet<TEntity> _Set => _set ??= _context.Set<TEntity>();

    public Task<IQueryable<TEntity>> QueryAsync(Inclusion<TKey, TEntity>[]? includes = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = _includableEvaluator.Evaluate<TEntity, TKey>(_Set, includes ?? [], cancellationToken);

        return Task.FromResult(queryable);
    }

    public async Task<Result<TEntity>> FindAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await _Set.FindAsync([id], cancellationToken);

        if (entity is null)
        {
            return Result<TEntity>.Failure(Errors.EntityNotFoundError);
        }

        return Result<TEntity>.Success(entity);
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
        entity.CreatedDate = DateTime.UtcNow;
        entity.UpdatedDate = entity.CreatedDate;

        var entry = await _Set.AddAsync(entity, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return entry.Entity;
    }

    public async Task InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        var entitiesList = entities as List<TEntity> ?? entities.ToList();

        var createdTime = DateTime.UtcNow;
        foreach (var entity in entitiesList)
        {
            entity.CreatedDate = createdTime;
            entity.UpdatedDate = createdTime;
        }

        await _Set.AddRangeAsync(entitiesList, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task DeleteAsync(IQueryable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        return entities.ExecuteDeleteAsync(cancellationToken);
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

    public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedDate = DateTime.UtcNow;

        var entry = _Set.Update(entity);

        entry.Property(e => e.CreatedDate).IsModified = false;

        await _context.SaveChangesAsync(cancellationToken);

        return entry.Entity;
    }

    public Task<int> UpdateAsync(Expression<Func<TEntity, bool>> predicate, Update<TEntity>[] updates,
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
        
        validUpdates.Add(Update<TEntity>.With(e => e.UpdatedDate, DateTime.UtcNow));

        var methods = typeof(SetPropertyCalls<TEntity>)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public);

        var m = methods.Single(methodInfo =>
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

        return _Set.Where(predicate)
            .ExecuteUpdateAsync(updateExpression, cancellationToken);
    }
}