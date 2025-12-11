using System.Linq.Expressions;
using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.InfrastructureTypes;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Data;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;

public interface IRepository<TKey, TEntity>
    where TEntity : IBaseEntity<TKey>
    where TKey : IParsable<TKey>,
    IEquatable<TKey>
{
    Task<IQueryable<TEntity>> QueryAsync(Inclusion<TKey, TEntity>[]? includes = null, CancellationToken cancellationToken = default);

    Task<Result<TEntity>> FindAsync(TKey id, Inclusion<TKey, TEntity>[]? includes = null, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> condition, Inclusion<TKey, TEntity>[]? includes = null,
        CancellationToken cancellationToken = default);

    Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task DeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    
    Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, Inclusion<TKey, TEntity>[]? includes = null, CancellationToken cancellationToken = default);

    Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);

    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task UpdateAsync(TEntity entity, Update<TEntity>[] updates, CancellationToken cancellationToken = default);

    Task<int> UpdateAsync(Expression<Func<TEntity, bool>> predicate, Update<TEntity>[] updates,
        CancellationToken cancellationToken = default);
}