using System.Diagnostics;
using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.InfrastructureTypes;
using FlorianAlbert.FinanceObserver.Server.CrossCutting.Model;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.EmailManagement.Contract;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Business.Identity.EmailSending;

public class IdentityEmailSender : IEmailSender
{
    private static readonly ActivitySource _activity = new(typeof(IdentityEmailSender).FullName ?? nameof(IdentityEmailSender));
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public IdentityEmailSender(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        using Activity? activity = _activity.StartActivity("IdentityEmailSender.SendEmail", ActivityKind.Internal);
        activity?.SetTag("email.subject", subject);

        await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();

        ILogger<IdentityEmailSender> logger = scope.ServiceProvider.GetRequiredService<ILogger<IdentityEmailSender>>();

        IEmailManager emailManager = scope.ServiceProvider.GetRequiredService<IEmailManager>();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        
        User? user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "User not found");
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError("Tried to send email to non existing user.");
            }

            return;
        }

        Email emailToSend = new()
        {
            Id = Guid.Empty,
            Receivers = [user],
            Subject = subject,
            Message = htmlMessage
        };

        Result sendResult = await emailManager.SendEmailAsync(emailToSend);

        if (sendResult.Failed)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Email send failed");
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError("Failed to send email. Errors: {Errors}", string.Join(", ", sendResult.Errors));
            }
        }
        else
        {
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
    }
}