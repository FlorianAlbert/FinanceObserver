using Aspire.FluentEmail.MailKit;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.EmailManagement;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.EmailManagement.Contract;

namespace FlorianAlbert.FinanceObserver.Server.Startup.ServiceInstallers;

internal class EmailManagementInstaller : IServiceInstaller
{
    private const string _fromEmailAddressEnvKey = "FINANCE_OBSERVER_FROM_EMAIL_ADDRESS";
    private const string _fromEmailAddressFileEnvKey = "FINANCE_OBSERVER_FROM_EMAIL_ADDRESS_FILE";
    private const string _fromEmailNameEnvKey = "FINANCE_OBSERVER_FROM_EMAIL_NAME";
    private const string _fromEmailNameFileEnvKey = "FINANCE_OBSERVER_FROM_EMAIL_NAME_FILE";

    public void Install(IHostApplicationBuilder builder, ILogger logger)
    {
        logger.LogInformation("Adding email management");
        
        // Read settings
        string? fromEmailAddress = null;
        if (Environment.GetEnvironmentVariable(_fromEmailAddressFileEnvKey) is { } fromEmailAddressFileLocation
            && File.Exists(fromEmailAddressFileLocation))
        {
            fromEmailAddress = File.ReadAllText(fromEmailAddressFileLocation);
        }
        fromEmailAddress ??= Environment.GetEnvironmentVariable(_fromEmailAddressEnvKey)
                             ?? builder.Configuration["SmtpSettings:FromEmail"];
        ArgumentException.ThrowIfNullOrEmpty(fromEmailAddress);

        string? fromEmailName = null;
        if (Environment.GetEnvironmentVariable(_fromEmailNameFileEnvKey) is { } fromEmailNameFileLocation
            && File.Exists(fromEmailNameFileLocation))
        {
            fromEmailName = File.ReadAllText(fromEmailNameFileLocation);
        }
        fromEmailName ??= Environment.GetEnvironmentVariable(_fromEmailNameEnvKey)
                          ?? builder.Configuration["SmtpSettings:FromName"];
        ArgumentException.ThrowIfNullOrEmpty(fromEmailName);

        builder.AddFluentEmail("maildev", fromEmailAddress, fromEmailName);

        //Register services
        builder.Services.AddScoped<IEmailManager, EmailManager>();
    }
}