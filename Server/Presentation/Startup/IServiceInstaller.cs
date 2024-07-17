namespace FlorianAlbert.FinanceObserver.Server.Startup;

internal interface IServiceInstaller
{
    void Install(IHostApplicationBuilder builder, ILogger logger);
}