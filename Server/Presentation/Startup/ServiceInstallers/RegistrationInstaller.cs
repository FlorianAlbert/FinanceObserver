using FlorianAlbert.FinanceObserver.Server.Logic.Business.RegistrationWorkflow;
using FlorianAlbert.FinanceObserver.Server.Logic.Business.RegistrationWorkflow.Contract;

namespace FlorianAlbert.FinanceObserver.Server.Startup.ServiceInstallers;

internal class RegistrationInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration, ILogger logger)
    {
        logger.LogInformation("Adding registration workflow");
        
        services.AddTransient<IRegistrationWorkflow, RegistrationWorkflow>();

        services.AddHostedService<ExpiredRegistrationsUserDeletionService>();
    }
}