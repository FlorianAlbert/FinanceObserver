using FlorianAlbert.FinanceObserver.Server.Presentation.REST;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;

namespace FlorianAlbert.FinanceObserver.Server.Startup.ServiceInstallers;

internal class ControllersInstaller : IServiceInstaller
{
    public void Install(IHostApplicationBuilder builder, ILogger logger)
    {
        logger.LogInformation("Adding controllers");

        builder.Services.AddAuthorization();
        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 4;
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApi(options =>
        {
            options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
        });
    }
}