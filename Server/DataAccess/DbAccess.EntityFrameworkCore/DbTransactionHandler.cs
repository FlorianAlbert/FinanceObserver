using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore;

public class DbTransactionHandler : IDbTransactionHandler
{
    private readonly FinanceObserverContext _context;

    public DbTransactionHandler(FinanceObserverContext context)
    {
        _context = context;
    }

    public async Task<IAsyncDisposable> StartTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.CommitTransactionAsync(cancellationToken);
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        return _context.Database.RollbackTransactionAsync(cancellationToken);
    }
}