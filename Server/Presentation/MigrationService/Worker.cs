using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FlorianAlbert.FinanceObserver.Server.MigrationService;

public class Worker : BackgroundService
{
    public Worker(IServiceProvider serviceProvider, 
        IHostApplicationLifetime hostApplicationLifetime)
    {
        _serviceProvider = serviceProvider;
        _hostApplicationLifetime = hostApplicationLifetime;
    }

    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource _activitySource = new(ActivitySourceName);
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    protected override async Task ExecuteAsync(
        CancellationToken cancellationToken)
    {
        using Activity? activity = _activitySource.StartActivity(
            "Migrating database", ActivityKind.Client);

        try
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            DbContext dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();

            await RunMigrationAsync(dbContext, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            throw;
        }

        _hostApplicationLifetime.StopApplication();
    }

    private static async Task RunMigrationAsync(
        DbContext dbContext, CancellationToken cancellationToken)
    {
        IExecutionStrategy strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Run migration in a transaction to avoid partial migration if it fails.
            await dbContext.Database.MigrateAsync(cancellationToken);
        });
    }
}