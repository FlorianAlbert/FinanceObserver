using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Business.Identity.EmailSending.Extensions;

public static class HostApplicationBuilderExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder AddIdentityEmailSending()
        {
            builder.Services.AddTransient<IEmailSender, IdentityEmailSender>();

            builder.Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing.AddSource(IdentityEmailSender.ActivitySourceName);
            });

            return builder;
        }
    }
}
