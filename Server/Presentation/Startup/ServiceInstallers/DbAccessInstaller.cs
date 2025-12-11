using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Data;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FlorianAlbert.FinanceObserver.Server.Startup.ServiceInstallers;

internal class DbAccessInstaller : IServiceInstaller
{
    private const string _dbProviderEnvKey = "FINANCE_OBSERVER_DB_PROVIDER";
    private const string _dbProviderFileEnvKey = "FINANCE_OBSERVER_DB_PROVIDER_FILE";
    private const string _dbConnectionNameEnvKey = "FINANCE_OBSERVER_DB_CONNECTIONNAME";
    private const string _dbConnectionNameFileEnvKey = "FINANCE_OBSERVER_DB_CONNECTIONNAME_FILE";

    public void Install(IHostApplicationBuilder builder, ILogger logger)
    {
        logger.LogInformation("Adding database access");
        
        string? dbProvider = null;
        if (Environment.GetEnvironmentVariable(_dbProviderFileEnvKey) is { } dbProviderFileLocation
            && File.Exists(dbProviderFileLocation))
        {
            dbProvider = File.ReadAllText(dbProviderFileLocation);
        }

        dbProvider ??= Environment.GetEnvironmentVariable(_dbProviderEnvKey);
        ArgumentException.ThrowIfNullOrEmpty(dbProvider);

        if (!Enum.TryParse(dbProvider, true, out DatabaseProvider provider))
        {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            throw new ArgumentOutOfRangeException(nameof(provider), "There was no valid Database provider given");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
        }

        string? dbConnectionName = null;
        if (Environment.GetEnvironmentVariable(_dbConnectionNameFileEnvKey) is { } dbConnectionNameFileLocation
            && File.Exists(dbConnectionNameFileLocation))
        {
            dbConnectionName = File.ReadAllText(dbConnectionNameFileLocation);
        }

        dbConnectionName ??= Environment.GetEnvironmentVariable(_dbConnectionNameEnvKey);
        ArgumentException.ThrowIfNullOrEmpty(dbConnectionName);

        switch (provider)
        {
            case DatabaseProvider.Npgsql:
                builder.AddNpgsqlDbContext<FinanceObserverContext>(dbConnectionName,
                    efCorePostgresSettings =>
                    {
                        efCorePostgresSettings.DisableRetry = true;
                    },
                    contextOptionsBuilder =>
                    {
                        contextOptionsBuilder.UseNpgsql(npgsqlOptionsBuilder => npgsqlOptionsBuilder.MigrationsAssembly(typeof(AssemblyReference).Assembly.FullName));
                    });
                break;
            default:
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                throw new ArgumentOutOfRangeException(nameof(provider), "There was no valid Database provider given");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
        }

        builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<FinanceObserverContext>());

        builder.Services.AddIdentityApiEndpoints<User>()
            .AddEntityFrameworkStores<FinanceObserverContext>();

        builder.Services.AddScoped<IRepositoryFactory, RepositoryFactory>();
        builder.Services.AddScoped<IDbTransactionHandler, DbTransactionHandler>();
    }
}