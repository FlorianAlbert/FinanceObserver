using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore;

public class RepositoryFactory : IRepositoryFactory
{
    private readonly FinanceObserverContext _dbContext;

    private readonly IInclusionEvaluator _inclusionEvaluator;

    public RepositoryFactory(FinanceObserverContext dbContext, IInclusionEvaluator inclusionEvaluator)
    {
        _dbContext = dbContext;
        _inclusionEvaluator = inclusionEvaluator;
    }

    public IRepository<TKey, TEntity> CreateRepository<TKey, TEntity>()
        where TKey : IParsable<TKey>, IEquatable<TKey>
        where TEntity : BaseEntity<TKey>
    {
        return new Repository<TKey, TEntity>(_dbContext, _inclusionEvaluator);
    }
}