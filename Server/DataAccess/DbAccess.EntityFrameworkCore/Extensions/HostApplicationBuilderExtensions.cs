using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.Extensions;

public static class HostApplicationBuilderExtensions
{
    private const string _dbConnectionNameEnvKey = "FINANCE_OBSERVER_DB_CONNECTIONNAME";
    private const string _dbConnectionNameFileEnvKey = "FINANCE_OBSERVER_DB_CONNECTIONNAME_FILE";

    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder AddEntityFrameworkCoreDbAccess()
        {
            string? dbConnectionName = null;
            if (Environment.GetEnvironmentVariable(_dbConnectionNameFileEnvKey) is { } dbConnectionNameFileLocation
                && File.Exists(dbConnectionNameFileLocation))
            {
                dbConnectionName = File.ReadAllText(dbConnectionNameFileLocation);
            }

            dbConnectionName ??= Environment.GetEnvironmentVariable(_dbConnectionNameEnvKey);
            ArgumentException.ThrowIfNullOrEmpty(dbConnectionName);

            builder.AddNpgsqlDbContext<FinanceObserverContext>(dbConnectionName,
                efCorePostgresSettings =>
                {
                    efCorePostgresSettings.DisableRetry = true;
                },
                contextOptionsBuilder =>
                {
                    contextOptionsBuilder.UseNpgsql(npgsqlOptionsBuilder => npgsqlOptionsBuilder.MigrationsAssembly(typeof(AssemblyReference).Assembly.FullName));
                });

            builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<FinanceObserverContext>());

            builder.Services.AddIdentityApiEndpoints<User>()
                .AddEntityFrameworkStores<FinanceObserverContext>();

            builder.Services.AddScoped<IRepositoryFactory, RepositoryFactory>();
            builder.Services.AddScoped<IDbTransactionHandler, DbTransactionHandler>();

            return builder;
        }
    }
}
