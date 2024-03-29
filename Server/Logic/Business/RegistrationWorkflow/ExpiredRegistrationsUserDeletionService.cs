using FlorianAlbert.FinanceObserver.Server.Logic.Domain.RegistrationConfirmationManagement.Contract;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.UserManagement.Contract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Business.RegistrationWorkflow;

public class ExpiredRegistrationsUserDeletionService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly PeriodicTimer _timer;

    public ExpiredRegistrationsUserDeletionService(IServiceProvider serviceProvider, int executionPeriodInSeconds)
    {
        _serviceProvider = serviceProvider;
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(executionPeriodInSeconds));
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (await _timer.WaitForNextTickAsync(cancellationToken) &&
               !cancellationToken.IsCancellationRequested)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            
            var registrationConfirmationManager = scope.ServiceProvider.GetRequiredService<IRegistrationConfirmationManager>();
            var userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
            
            var unconfirmedRegistrationConfirmationsResult =
                await registrationConfirmationManager.GetUnconfirmedRegistrationConfirmationsWithUserAsync(
                    cancellationToken);

            if (unconfirmedRegistrationConfirmationsResult.Failed)
            {
                continue;
            }

            var unconfirmedRegistrationConfirmations = unconfirmedRegistrationConfirmationsResult.Value;

            var expiredUsers =
                unconfirmedRegistrationConfirmations.Where(r =>
                    r.CreatedDate.AddDays(1) <= DateTimeOffset.UtcNow).Select(r => r.User);

            await userManager.RemoveUsersAsync(expiredUsers, cancellationToken);
        }
    }
}