namespace FlorianAlbert.FinanceObserver.Server.Startup;

internal interface IServiceInstaller
{
    void Install(IServiceCollection services, IConfiguration configuration, ILogger logger);
}