using Aspire.FluentEmail.MailKit;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.EmailManagement;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.EmailManagement.Contract;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.EmailManagement.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.EmailManagement.Extensions;

public static class HostApplicationBuilderExtensions
{
    private const string _smtpConnectionNameEnvKey = "FINANCE_OBSERVER_SMTP_CONNECTIONNAME";
    private const string _smtpConnectionNameFileEnvKey = "FINANCE_OBSERVER_SMTP_CONNECTIONNAME_FILE";
    private const string _fromEmailAddressEnvKey = "FINANCE_OBSERVER_FROM_EMAIL_ADDRESS";
    private const string _fromEmailAddressFileEnvKey = "FINANCE_OBSERVER_FROM_EMAIL_ADDRESS_FILE";
    private const string _fromEmailNameEnvKey = "FINANCE_OBSERVER_FROM_EMAIL_NAME";
    private const string _fromEmailNameFileEnvKey = "FINANCE_OBSERVER_FROM_EMAIL_NAME_FILE";

    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder AddFluentEmailManagement()
        {
            // Read settings
            string? smtpConnectionName = null;
            if (Environment.GetEnvironmentVariable(_smtpConnectionNameFileEnvKey) is { } smtpConnectionNameFileLocation
                && File.Exists(smtpConnectionNameFileLocation))
            {
                smtpConnectionName = File.ReadAllText(smtpConnectionNameFileLocation);
            }
            smtpConnectionName ??= Environment.GetEnvironmentVariable(_smtpConnectionNameEnvKey);
            ArgumentException.ThrowIfNullOrEmpty(smtpConnectionName);

            string? fromEmailAddress = null;
            if (Environment.GetEnvironmentVariable(_fromEmailAddressFileEnvKey) is { } fromEmailAddressFileLocation
                && File.Exists(fromEmailAddressFileLocation))
            {
                fromEmailAddress = File.ReadAllText(fromEmailAddressFileLocation);
            }
            fromEmailAddress ??= Environment.GetEnvironmentVariable(_fromEmailAddressEnvKey);
            ArgumentException.ThrowIfNullOrEmpty(fromEmailAddress);

            string? fromEmailName = null;
            if (Environment.GetEnvironmentVariable(_fromEmailNameFileEnvKey) is { } fromEmailNameFileLocation
                && File.Exists(fromEmailNameFileLocation))
            {
                fromEmailName = File.ReadAllText(fromEmailNameFileLocation);
            }
            fromEmailName ??= Environment.GetEnvironmentVariable(_fromEmailNameEnvKey);
            ArgumentException.ThrowIfNullOrEmpty(fromEmailName);

            builder.AddFluentEmail(smtpConnectionName, fromEmailAddress, fromEmailName);

            builder.Services.AddScoped<IEmailManager, EmailManager>();

            builder.Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing.AddSource(typeof(EmailManager).FullName ?? nameof(EmailManager));
            });

            return builder;
        }
    }
}
