using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Data;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FlorianAlbert.FinanceObserver.Server.Startup.ServiceInstallers;

internal class DbAccessInstaller : IServiceInstaller
{
    private const string _dbProviderEnvKey = "FINANCE_OBSERVER_DB_PROVIDER";
    private const string _dbProviderFileEnvKey = "FINANCE_OBSERVER_DB_PROVIDER_FILE";
    private const string _dbProviderKey = "DbAccess:ActiveProvider";
    private const string _dbConnectionStringEnvKey = "FINANCE_OBSERVER_DB_CONNECTIONSTRING";
    private const string _dbConnectionStringFileEnvKey = "FINANCE_OBSERVER_DB_CONNECTIONSTRING_FILE";
    private const string _dbConfiguredProvidersKey = "DbAccess:ConfiguredProviders";

    public void Install(IHostApplicationBuilder builder, ILogger logger)
    {
        logger.LogInformation("Adding database access");
        
        string? dbProvider = null;
        if (Environment.GetEnvironmentVariable(_dbProviderFileEnvKey) is { } dbProviderFileLocation
            && File.Exists(dbProviderFileLocation))
        {
            dbProvider = File.ReadAllText(dbProviderFileLocation);
        }

        dbProvider ??= Environment.GetEnvironmentVariable(_dbProviderEnvKey)
                       ?? builder.Configuration[_dbProviderKey];
        ArgumentException.ThrowIfNullOrEmpty(dbProvider);

        string? dbConnectionString = null;
        if (Environment.GetEnvironmentVariable(_dbConnectionStringFileEnvKey) is { } dbConnectionStringFileLocation
            && File.Exists(dbConnectionStringFileLocation))
        {
            dbConnectionString = File.ReadAllText(dbConnectionStringFileLocation);
        }

        dbConnectionString ??= Environment.GetEnvironmentVariable(_dbConnectionStringEnvKey);

        if (!Enum.TryParse(dbProvider, true, out DatabaseProvider provider))
        {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            throw new ArgumentOutOfRangeException(nameof(provider), "There was no valid Database provider given");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
        }

        if (dbConnectionString is null)
        {
            if (!(builder.Configuration.GetSection(_dbConfiguredProvidersKey).GetChildren() is { } providers
                  && providers.Any(p => p.Key == dbProvider)))
            {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                throw new ArgumentOutOfRangeException(nameof(provider), "There was no valid Database provider given");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            }

            dbConnectionString = builder.Configuration[_dbConfiguredProvidersKey + ":" + provider]!;
        }

        builder.AddNpgsqlDbContext<FinanceObserverContext>("postgres",
            efCorePostgresSettings =>
            {
                efCorePostgresSettings.DisableRetry = true;
            },
            contextOptionsBuilder =>
            {
                contextOptionsBuilder.UseNpgsql(npgsqlOptionsBuilder => npgsqlOptionsBuilder.MigrationsAssembly(typeof(AssemblyReference).Assembly.FullName));
            });

        builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<FinanceObserverContext>());

        builder.Services.AddScoped<IRepositoryFactory, RepositoryFactory>();
        builder.Services.AddScoped<IDbTransactionHandler, DbTransactionHandler>();
    }
}