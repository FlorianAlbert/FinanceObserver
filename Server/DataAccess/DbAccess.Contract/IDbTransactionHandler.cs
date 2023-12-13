namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;

public interface IDbTransactionHandler
{
    Task<IAsyncDisposable> StartTransactionAsync(CancellationToken cancellationToken = default);

    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}