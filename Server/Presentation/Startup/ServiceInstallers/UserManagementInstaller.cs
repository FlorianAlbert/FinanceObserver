using FlorianAlbert.FinanceObserver.Server.Logic.Domain.UserManagement;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.UserManagement.Contract;

namespace FlorianAlbert.FinanceObserver.Server.Startup.ServiceInstallers;

internal class UserManagementInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration, ILogger logger)
    {
        logger.LogInformation("Adding user management");
        
        services.AddScoped<IUserManager, UserManager>();
    }
}