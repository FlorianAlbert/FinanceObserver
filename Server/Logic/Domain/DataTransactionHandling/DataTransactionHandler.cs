using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.DataTransactionHandling.Contract;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.DataTransactionHandling;

public class DataTransactionHandler : IDataTransactionHandler
{
    private readonly IDbTransactionHandler _dbTransactionHandler;

    public DataTransactionHandler(IDbTransactionHandler dbTransactionHandler)
    {
        _dbTransactionHandler = dbTransactionHandler;
    }

    public Task CommitDbTransactionAsync(CancellationToken cancellationToken = default)
    {
        return _dbTransactionHandler.CommitTransactionAsync(cancellationToken);
    }

    public Task RollbackDbTransactionAsync(CancellationToken cancellationToken = default)
    {
        return _dbTransactionHandler.RollbackTransactionAsync(cancellationToken);
    }

    public Task StartDbTransactionAsync(CancellationToken cancellationToken = default)
    {
        return _dbTransactionHandler.StartTransactionAsync(cancellationToken);
    }
}