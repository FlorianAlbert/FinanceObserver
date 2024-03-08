using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;
using Microsoft.EntityFrameworkCore;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore;

public class RepositoryFactory : IRepositoryFactory
{
    private readonly DbContext _dbContext;

    public RepositoryFactory(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IRepository<TKey, TEntity> CreateRepository<TKey, TEntity>()
        where TKey : IParsable<TKey>, IEquatable<TKey>
        where TEntity : BaseEntity<TKey>
    {
        return new Repository<TKey, TEntity>(_dbContext,
            new InclusionEvaluator());
    }
}