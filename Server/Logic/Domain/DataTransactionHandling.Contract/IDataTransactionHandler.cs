namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.DataTransactionHandling.Contract;

public interface IDataTransactionHandler
{
    Task StartDbTransactionAsync(CancellationToken cancellationToken = default);

    Task CommitDbTransactionAsync(CancellationToken cancellationToken = default);

    Task RollbackDbTransactionAsync(CancellationToken cancellationToken = default);
}