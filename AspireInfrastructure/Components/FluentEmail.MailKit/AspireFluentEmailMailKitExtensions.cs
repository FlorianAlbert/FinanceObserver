using FluentEmail.MailKitSmtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Aspire.FluentEmail.MailKit;

public static class AspireFluentEmailMailKitExtensions
{
    private const string _hostEnvKey = "FINANCE_OBSERVER_SMTP_CONNECTION_STRING";
    private const string _hostFileEnvKey = "FINANCE_OBSERVER_SMTP_CONNECTION_STRING_FILE";

    public static IHostApplicationBuilder AddFluentEmail(
        this IHostApplicationBuilder builder,
        string connectionName,
        string fromEmailAddress,
        string fromEmailName)
    {
        string? smtpConnectionString = null;
        if (Environment.GetEnvironmentVariable(_hostFileEnvKey) is { } smtpConnectionStringFileLocation
            && File.Exists(smtpConnectionStringFileLocation))
        {
            smtpConnectionString = File.ReadAllText(smtpConnectionStringFileLocation);
        }
        smtpConnectionString ??= Environment.GetEnvironmentVariable(_hostEnvKey);
        smtpConnectionString ??= builder.Configuration["SmtpSettings:ConnectionString"];
        smtpConnectionString ??= builder.Configuration.GetConnectionString(connectionName);
        ArgumentException.ThrowIfNullOrEmpty(smtpConnectionString);

        var smtpConnectionStringUri = new Uri(smtpConnectionString);
        if (!string.IsNullOrEmpty(smtpConnectionStringUri.UserInfo))
        {
            string[] userInfos = smtpConnectionStringUri.UserInfo.Split(':',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            builder.Services.AddFluentEmail(fromEmailAddress, fromEmailName)
                .AddMailKitSender(new SmtpClientOptions
                {
                    Server = smtpConnectionStringUri.Host,
                    Port = smtpConnectionStringUri.Port,
                    User = userInfos[0],
                    Password = userInfos[1],
                    RequiresAuthentication = true,
                    UseSsl = false
                });
        }
        else
        {
            builder.Services.AddFluentEmail(fromEmailAddress, fromEmailName)
                .AddMailKitSender(new SmtpClientOptions
                {
                    Server = smtpConnectionStringUri.Host,
                    Port = smtpConnectionStringUri.Port,
                    UseSsl = false
                });
        }

        return builder;
    }

    public static IHostApplicationBuilder AddIdentityEmailSender(
        this IHostApplicationBuilder builder)
    {
        builder.Services.AddTransient<IEmailSender, FluentEmailSender>();
        return builder;
    }

}
