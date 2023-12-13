using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;

public interface IRepositoryFactory
{
    IRepository<TKey, TEntity> CreateRepository<TKey, TEntity>()
        where TKey : IParsable<TKey>,
        IEquatable<TKey>
        where TEntity : BaseEntity<TKey>;
}