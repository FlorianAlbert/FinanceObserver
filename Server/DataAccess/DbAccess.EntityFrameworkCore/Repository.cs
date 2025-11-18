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
        var queryable = _inclusionEvaluator.Evaluate(_Set.AsNoTracking(), includes ?? [], cancellationToken);

        return Task.FromResult(queryable);
    }

    public async Task<Result<TEntity>> FindAsync(TKey id, Inclusion<TKey, TEntity>[]? includes = null, CancellationToken cancellationToken = default)
    {
        var entity = (await QueryAsync(includes, cancellationToken)).SingleOrDefault(entity => entity.Id.Equals(id));

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
        var createdTime = DateTimeOffset.UtcNow;

        var newEntry = _context.Attach(entity);
        newEntry.State = EntityState.Added;

        foreach (var entry in _context.ChangeTracker.Entries().Where(entry => entry.State == EntityState.Added))
        {
            if (entry.Entity is LifecycleTrackable lifecycleTrackable)
            {
               lifecycleTrackable.CreatedDate = createdTime;
               lifecycleTrackable.UpdatedDate = createdTime;
            }
        }

        foreach (var entry in _context.ChangeTracker.Entries().Where(entry => entry.State == EntityState.Modified))
        {
            if (entry.Entity is LifecycleTrackable lifecycleTrackable)
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
        var entitiesArray = entities as TEntity[] ?? entities.ToArray();

        var createdTime = DateTimeOffset.UtcNow;
        foreach (var entity in entitiesArray)
        {
            var entry = _context.Attach(entity);
            entry.State = EntityState.Added;
        }

        foreach (var entry in _context.ChangeTracker.Entries().Where(entry => entry.State == EntityState.Added))
        {
            if (entry.Entity is LifecycleTrackable lifecycleTrackable)
            {
                lifecycleTrackable.CreatedDate = createdTime;
                lifecycleTrackable.UpdatedDate = createdTime;
            }
        }

        foreach (var entry in _context.ChangeTracker.Entries().Where(entry => entry.State == EntityState.Modified))
        {
            if (entry.Entity is LifecycleTrackable lifecycleTrackable)
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

        var entitiesArray = entities as object[] ?? entities.ToArray();

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

        var entitiesToUpdate = _Set.Where(predicate);
        
        var updatedRowsCount = await entitiesToUpdate.ExecuteUpdateAsync(builder => 
        {
            foreach (var item in validUpdates)
            {
                builder.SetProperty(item.SelectorExpression, item.ValueExpression);
            }
        }, cancellationToken: cancellationToken);

        return updatedRowsCount;
    }
}