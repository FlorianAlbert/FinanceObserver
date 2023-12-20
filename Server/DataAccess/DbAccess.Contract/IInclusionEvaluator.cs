using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Data;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;

public interface IInclusionEvaluator
{
    IQueryable<TEntity> Evaluate<TEntity, TKey>(IQueryable<TEntity> queryable,
        Inclusion<TKey, TEntity>[] inclusions,
        CancellationToken cancellationToken = default)
        where TEntity : BaseEntity<TKey>
        where TKey : IParsable<TKey>,
        IEquatable<TKey>;
}