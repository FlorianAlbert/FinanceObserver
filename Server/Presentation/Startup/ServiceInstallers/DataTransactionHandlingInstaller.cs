using FlorianAlbert.FinanceObserver.Server.Logic.Domain.DataTransactionHandling;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.DataTransactionHandling.Contract;

namespace FlorianAlbert.FinanceObserver.Server.Startup.ServiceInstallers;

public class DataTransactionHandlingInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration, ILogger logger)
    {
        logger.LogInformation("Adding data transaction handling");
        
        services.AddScoped<IDataTransactionHandler, DataTransactionHandler>();
    }
}