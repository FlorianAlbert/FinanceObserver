using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.InfrastructureTypes;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.EmailManagement.Contract;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Business.Identity.EmailSending;

public class IdentityEmailSender : IEmailSender
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public IdentityEmailSender(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();

        ILogger<IdentityEmailSender> logger = scope.ServiceProvider.GetRequiredService<ILogger<IdentityEmailSender>>();

        IEmailManager emailManager = scope.ServiceProvider.GetRequiredService<IEmailManager>();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        
        User? user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError("Tried to send email to non existing user with email {Email}.", email);
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

        if (sendResult.Failed && logger.IsEnabled(LogLevel.Error))
        {
            logger.LogError("Failed to send email to {Email}. Errors: {Errors}", email, string.Join(", ", sendResult.Errors));
        }
    }
}