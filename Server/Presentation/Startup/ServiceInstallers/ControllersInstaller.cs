using FlorianAlbert.FinanceObserver.Server.Presentation.REST;

namespace FlorianAlbert.FinanceObserver.Server.Startup.ServiceInstallers;

internal class ControllersInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration, ILogger logger)
    {
        logger.LogInformation("Adding controllers");
        
        services.AddControllers()
            .AddApplicationPart(typeof(AssemblyReference).Assembly);

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            var xmlDocumentationPath = Path.Combine(AppContext.BaseDirectory,
                $"{typeof(AssemblyReference).Assembly.GetName().Name}.xml");
            options.IncludeXmlComments(xmlDocumentationPath, true);
        });
    }
}