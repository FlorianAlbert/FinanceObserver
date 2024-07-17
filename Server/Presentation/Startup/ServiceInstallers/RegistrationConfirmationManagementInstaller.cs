using FlorianAlbert.FinanceObserver.Server.Logic.Domain.RegistrationConfirmationManagement;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.RegistrationConfirmationManagement.Contract;

namespace FlorianAlbert.FinanceObserver.Server.Startup.ServiceInstallers;

public class RegistrationConfirmationManagementInstaller : IServiceInstaller
{
    public void Install(IHostApplicationBuilder builder, ILogger logger)
    {
        logger.LogInformation("Adding RegistrationConfirmation management");
        
        builder.Services.AddScoped<IRegistrationConfirmationManager, RegistrationConfirmationManager>();
    }
}