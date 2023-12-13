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

    public void Install(IServiceCollection services, IConfiguration configuration, ILogger logger)
    {
        logger.LogInformation("Adding database access");
        
        string? dbProvider = null;
        if (Environment.GetEnvironmentVariable(_dbProviderFileEnvKey) is { } dbProviderFileLocation
            && File.Exists(dbProviderFileLocation))
        {
            dbProvider = File.ReadAllText(dbProviderFileLocation);
        }

        dbProvider ??= Environment.GetEnvironmentVariable(_dbProviderEnvKey)
                       ?? configuration[_dbProviderKey];
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
            if (!(configuration.GetSection(_dbConfiguredProvidersKey).GetChildren() is { } providers
                  && providers.Any(p => p.Key == dbProvider)))
            {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                throw new ArgumentOutOfRangeException(nameof(provider), "There was no valid Database provider given");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            }

            dbConnectionString = configuration[_dbConfiguredProvidersKey + ":" + provider]!;
        }
        
        logger.LogTrace("DatabaseProvider: {DatabaseProvider}", provider);
        logger.LogTrace("Database ConnectionString: {DatabaseConnectionString}", dbConnectionString);

        services.AddDbContextPool<FinanceObserverContext>(contextOptionsBuilder =>
        {
            switch (provider)
            {
                case DatabaseProvider.InMemory:
                    contextOptionsBuilder
                        .UseInMemoryDatabase(dbConnectionString, imdbo => { imdbo.EnableNullChecks(); })
                        .ConfigureWarnings(x => { x.Ignore(InMemoryEventId.TransactionIgnoredWarning); });
                    break;
                case DatabaseProvider.Npgsql:
                    contextOptionsBuilder.UseNpgsql(dbConnectionString);
                    break;
                case DatabaseProvider.None:
                default:
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                    throw new ArgumentOutOfRangeException(nameof(provider),
                        "The given Database provider is not supported");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            }
        });

        services.AddTransient<IInclusionEvaluator, InclusionEvaluator>();
        services.AddScoped<IRepositoryFactory, RepositoryFactory>();
        services.AddScoped<IDbTransactionHandler, DbTransactionHandler>();
    }
}