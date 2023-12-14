using FlorianAlbert.FinanceObserver.Server.Logic.Domain.EmailManagement;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.EmailManagement.Contract;
using FluentEmail.MailKitSmtp;

namespace FlorianAlbert.FinanceObserver.Server.Startup.ServiceInstallers;

internal class EmailManagementInstaller : IServiceInstaller
{
    private const string _fromEmailAddressEnvKey = "FINANCE_OBSERVER_FROM_EMAIL_ADDRESS";
    private const string _fromEmailAddressFileEnvKey = "FINANCE_OBSERVER_FROM_EMAIL_ADDRESS_FILE";
    private const string _fromEmailNameEnvKey = "FINANCE_OBSERVER_FROM_EMAIL_NAME";
    private const string _fromEmailNameFileEnvKey = "FINANCE_OBSERVER_FROM_EMAIL_NAME_FILE";
    private const string _hostEnvKey = "FINANCE_OBSERVER_SMTP_HOST";
    private const string _hostFileEnvKey = "FINANCE_OBSERVER_SMTP_HOST_FILE";
    private const string _portEnvKey = "FINANCE_OBSERVER_SMTP_PORT";
    private const string _portFileEnvKey = "FINANCE_OBSERVER_SMTP_PORT_FILE";
    private const string _usernameEnvKey = "FINANCE_OBSERVER_SMTP_USERNAME";
    private const string _usernameFileEnvKey = "FINANCE_OBSERVER_SMTP_USERNAME_FILE";
    private const string _passwordEnvKey = "FINANCE_OBSERVER_SMTP_PASSWORD";
    private const string _passwordFileEnvKey = "FINANCE_OBSERVER_SMTP_PASSWORD_FILE";

    public void Install(IServiceCollection services, IConfiguration configuration, ILogger logger)
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
                             ?? configuration["SmtpSettings:FromEmail"];
        ArgumentException.ThrowIfNullOrEmpty(fromEmailAddress);

        string? fromEmailName = null;
        if (Environment.GetEnvironmentVariable(_fromEmailNameFileEnvKey) is { } fromEmailNameFileLocation
            && File.Exists(fromEmailNameFileLocation))
        {
            fromEmailName = File.ReadAllText(fromEmailNameFileLocation);
        }

        fromEmailName ??= Environment.GetEnvironmentVariable(_fromEmailNameEnvKey)
                          ?? configuration["SmtpSettings:FromName"];
        ArgumentException.ThrowIfNullOrEmpty(fromEmailName);

        string? smtpHost = null;
        if (Environment.GetEnvironmentVariable(_hostFileEnvKey) is { } smtpHostFileLocation
            && File.Exists(smtpHostFileLocation))
        {
            smtpHost = File.ReadAllText(smtpHostFileLocation);
        }

        smtpHost ??= Environment.GetEnvironmentVariable(_hostEnvKey)
                     ?? configuration["SmtpSettings:Host"];
        ArgumentException.ThrowIfNullOrEmpty(smtpHost);

        string? smtpPortString = null;
        if (Environment.GetEnvironmentVariable(_portFileEnvKey) is { } smtpPortFileLocation
            && File.Exists(smtpPortFileLocation))
        {
            smtpPortString = File.ReadAllText(smtpPortFileLocation);
        }

        smtpPortString ??= Environment.GetEnvironmentVariable(_portEnvKey)
                           ?? configuration["SmtpSettings:Port"];
        ArgumentException.ThrowIfNullOrEmpty(smtpPortString);
        if (!int.TryParse(smtpPortString, out var smtpPort))
        {
            throw new StartupValidationException("There was no valid iteration count found in the configuration.");
        }

        string? smtpUsername = null;
        if (Environment.GetEnvironmentVariable(_usernameFileEnvKey) is { } smtpUsernameFileLocation
            && File.Exists(smtpUsernameFileLocation))
        {
            smtpUsername = File.ReadAllText(smtpUsernameFileLocation);
        }

        smtpUsername ??= Environment.GetEnvironmentVariable(_usernameEnvKey)
                         ?? configuration["SmtpSettings:Username"];

        string? smtpPassword = null;
        if (Environment.GetEnvironmentVariable(_passwordFileEnvKey) is { } smtpPasswordFileLocation
            && File.Exists(smtpPasswordFileLocation))
        {
            smtpPassword = File.ReadAllText(smtpPasswordFileLocation);
        }

        smtpPassword ??= Environment.GetEnvironmentVariable(_passwordEnvKey)
                         ?? configuration["SmtpSettings:Password"];

        //Register services
        services.AddScoped<IEmailManager, EmailManager>();

        if (!string.IsNullOrEmpty(smtpUsername) && !string.IsNullOrEmpty(smtpPassword))
        {
            services.AddFluentEmail(fromEmailAddress, fromEmailName)
                .AddMailKitSender(new SmtpClientOptions
                {
                    Server = smtpHost,
                    Port = smtpPort,
                    User = smtpUsername,
                    Password = smtpPassword,
                    RequiresAuthentication = true,
                    UseSsl = false
                });
        }
        else
        {
            services.AddFluentEmail(fromEmailAddress, fromEmailName)
                .AddMailKitSender(new SmtpClientOptions
                {
                    Server = smtpHost,
                    Port = smtpPort,
                    UseSsl = false
                });
        }
    }
}