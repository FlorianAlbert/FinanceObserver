using System.Linq.Expressions;
using System.Reflection;
using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.InfrastructureTypes;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Data;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore;

public class Repository<TKey, TEntity> : IRepository<TKey, TEntity>
    where TEntity : class, IBaseEntity<TKey>
    where TKey : IParsable<TKey>,
    IEquatable<TKey>
{
    private readonly DbContext _context;
    private readonly InclusionEvaluator _inclusionEvaluator;

    internal Repository(DbContext dbContext,
        InclusionEvaluator inclusionEvaluator)
    {
        _context = dbContext;
        _inclusionEvaluator = inclusionEvaluator;
    }

    private DbSet<TEntity> _Set => field ??= _context.Set<TEntity>();

    public Task<IQueryable<TEntity>> QueryAsync(Inclusion<TKey, TEntity>[]? includes = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> queryable = _inclusionEvaluator.Evaluate(_Set.AsNoTracking(), includes ?? [], cancellationToken);

        return Task.FromResult(queryable);
    }

    public async Task<Result<TEntity>> FindAsync(TKey id, Inclusion<TKey, TEntity>[]? includes = null, CancellationToken cancellationToken = default)
    {
        TEntity? entity = (await QueryAsync(includes, cancellationToken)).SingleOrDefault(entity => entity.Id.Equals(id));

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
        DateTimeOffset createdTime = DateTimeOffset.UtcNow;

        EntityEntry<TEntity> newEntry = _context.Attach(entity);
        newEntry.State = EntityState.Added;

        foreach (EntityEntry? entry in _context.ChangeTracker.Entries().Where(entry => entry.State == EntityState.Added))
        {
            if (entry.Entity is ILifecycleTrackable lifecycleTrackable)
            {
               lifecycleTrackable.CreatedDate = createdTime;
               lifecycleTrackable.UpdatedDate = createdTime;
            }
        }

        foreach (EntityEntry? entry in _context.ChangeTracker.Entries().Where(entry => entry.State == EntityState.Modified))
        {
            if (entry.Entity is ILifecycleTrackable lifecycleTrackable)
            {
                lifecycleTrackable.UpdatedDate = createdTime;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        
        _context.ChangeTracker.Clear();

        return newEntry.Entity;
    }

    public async Task InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        TEntity[] entitiesArray = entities as TEntity[] ?? [.. entities];

        DateTimeOffset createdTime = DateTimeOffset.UtcNow;
        foreach (TEntity entity in entitiesArray)
        {
            EntityEntry<TEntity> entry = _context.Attach(entity);
            entry.State = EntityState.Added;
        }

        foreach (EntityEntry? entry in _context.ChangeTracker.Entries().Where(entry => entry.State == EntityState.Added))
        {
            if (entry.Entity is ILifecycleTrackable lifecycleTrackable)
            {
                lifecycleTrackable.CreatedDate = createdTime;
                lifecycleTrackable.UpdatedDate = createdTime;
            }
        }

        foreach (EntityEntry? entry in _context.ChangeTracker.Entries().Where(entry => entry.State == EntityState.Modified))
        {
            if (entry.Entity is ILifecycleTrackable lifecycleTrackable)
            {
                lifecycleTrackable.UpdatedDate = createdTime;
            }
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

        object[] entitiesArray = entities as object[] ?? [.. entities];

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
                Name: nameof(IBaseEntity<>.UpdatedDate) or nameof(IBaseEntity<>.CreatedDate)
                or nameof(IBaseEntity<>.Id)
            }
        }).ToList();
        
        validUpdates.Add(Update<TEntity>.With(e => e.UpdatedDate, DateTimeOffset.UtcNow));

        IQueryable<TEntity> entitiesToUpdate = _Set.Where(predicate);

        int updatedRowsCount = await entitiesToUpdate.ExecuteUpdateAsync(builder =>
        {
            foreach (Update<TEntity>? item in validUpdates)
            {
#pragma warning disable EF1001 // Internal EF Core API usage.
                builder.SetProperty(item.SelectorExpression, item.ValueExpression);
#pragma warning restore EF1001 // Internal EF Core API usage.
            }
        }, cancellationToken: cancellationToken);

        return updatedRowsCount;
    }
}