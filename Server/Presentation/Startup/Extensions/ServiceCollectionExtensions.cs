using System.Reflection;

namespace FlorianAlbert.FinanceObserver.Server.Startup.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection InstallServices(this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] assemblies)
    {
        var serviceInstallers = assemblies.SelectMany(assembly => assembly.DefinedTypes)
            .Where(type => typeof(IServiceInstaller).IsAssignableFrom(type)
                           && type is { IsInterface: false, IsAbstract: false })
            .Select(Activator.CreateInstance)
            .Cast<IServiceInstaller>();

        foreach (var serviceInstaller in serviceInstallers)
        {
            var logger = services.BuildServiceProvider()
                .GetRequiredService(typeof(ILogger<>).MakeGenericType(serviceInstaller.GetType())) as ILogger;
            serviceInstaller.Install(services, configuration, logger!);
        }

        return services;
    }
}