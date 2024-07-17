using FlorianAlbert.FinanceObserver.Server.Logic.Domain.UserManagement;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.UserManagement.Contract;

namespace FlorianAlbert.FinanceObserver.Server.Startup.ServiceInstallers;

internal class UserManagementInstaller : IServiceInstaller
{
    public void Install(IHostApplicationBuilder builder, ILogger logger)
    {
        logger.LogInformation("Adding user management");
        
        builder.Services.AddScoped<IUserManager, UserManager>();
    }
}