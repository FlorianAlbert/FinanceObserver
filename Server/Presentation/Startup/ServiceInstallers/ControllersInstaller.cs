using FlorianAlbert.FinanceObserver.Server.Presentation.REST;

namespace FlorianAlbert.FinanceObserver.Server.Startup.ServiceInstallers;

internal class ControllersInstaller : IServiceInstaller
{
    public void Install(IHostApplicationBuilder builder, ILogger logger)
    {
        logger.LogInformation("Adding controllers");
        
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