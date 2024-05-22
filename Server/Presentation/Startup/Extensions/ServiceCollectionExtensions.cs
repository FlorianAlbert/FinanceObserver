using System.Reflection;

namespace FlorianAlbert.FinanceObserver.Server.Startup.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection InstallServices(this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] assemblies)
    {
        IEnumerable<IServiceInstaller> serviceInstallers = assemblies.SelectMany(assembly => assembly.DefinedTypes)
            .Where(type => typeof(IServiceInstaller).IsAssignableFrom(type)
                           && type is { IsInterface: false, IsAbstract: false })
            .Select(Activator.CreateInstance)
            .Cast<IServiceInstaller>();

        IServiceProvider serviceProvider = services.BuildServiceProvider();

        foreach (IServiceInstaller serviceInstaller in serviceInstallers)
        {
            var logger = serviceProvider
                .GetRequiredService(typeof(ILogger<>).MakeGenericType(serviceInstaller.GetType())) as ILogger;
            serviceInstaller.Install(services, configuration, logger!);
        }

        return services;
    }
}