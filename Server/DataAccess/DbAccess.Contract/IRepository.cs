using System.Linq.Expressions;
using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.InfrastructureTypes;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Data;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Data.Inclusion;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;

public interface IRepository<TKey, TEntity>
    where TEntity : BaseEntity<TKey>
    where TKey : IParsable<TKey>,
    IEquatable<TKey>
{
    Task<IQueryable<TEntity>> QueryAsync(Inclusion[]? includes = null, CancellationToken cancellationToken = default);

    Task<Result<TEntity>> FindAsync(TKey id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> condition, Inclusion[]? includes = null,
        CancellationToken cancellationToken = default);

    Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task DeleteAsync(IQueryable<TEntity> entities, CancellationToken cancellationToken = default);
    
    Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, Inclusion[]? includes = null, CancellationToken cancellationToken = default);

    Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);

    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<int> UpdateAsync(Expression<Func<TEntity, bool>> predicate, Update<TEntity>[] updates,
        CancellationToken cancellationToken = default);
}