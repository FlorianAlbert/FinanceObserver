using FlorianAlbert.FinanceObserver.Server.Logic.Domain.DataTransactionHandling;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.DataTransactionHandling.Contract;

namespace FlorianAlbert.FinanceObserver.Server.Startup.ServiceInstallers;

public class DataTransactionHandlingInstaller : IServiceInstaller
{
    public void Install(IHostApplicationBuilder builder, ILogger logger)
    {
        logger.LogInformation("Adding data transaction handling");
        
        builder.Services.AddScoped<IDataTransactionHandler, DataTransactionHandler>();
    }
}