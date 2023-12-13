using FlorianAlbert.FinanceObserver.Server.Logic.Domain.RegistrationConfirmationManagement;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.RegistrationConfirmationManagement.Contract;

namespace FlorianAlbert.FinanceObserver.Server.Startup.ServiceInstallers;

public class RegistrationConfirmationManagementInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration, ILogger logger)
    {
        logger.LogInformation("Adding RegistrationConfirmation management");
        
        services.AddScoped<IRegistrationConfirmationManager, RegistrationConfirmationManager>();
    }
}