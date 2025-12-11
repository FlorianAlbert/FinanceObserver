using FlorianAlbert.FinanceObserver.Server.Presentation.REST;
using Microsoft.AspNetCore.Identity;

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

        builder.Services.AddControllers()
            .AddApplicationPart(typeof(AssemblyReference).Assembly);

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            var xmlDocumentationPath = Path.Combine(AppContext.BaseDirectory,
                $"{typeof(AssemblyReference).Assembly.GetName().Name}.xml");
            options.IncludeXmlComments(xmlDocumentationPath, true);
        });
    }
}