using FluentEmail.MailKitSmtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Aspire.FluentEmail.MailKit;

public static class AspireFluentEmailMailKitExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder AddFluentEmail(
            string connectionName,
            string fromEmailAddress,
            string fromEmailName)
        {
            string? smtpConnectionString = builder.Configuration.GetConnectionString(connectionName);
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
    }
}
