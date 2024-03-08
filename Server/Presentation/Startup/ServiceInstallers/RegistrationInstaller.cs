using FlorianAlbert.FinanceObserver.Server.Logic.Business.RegistrationWorkflow;
using FlorianAlbert.FinanceObserver.Server.Logic.Business.RegistrationWorkflow.Contract;

namespace FlorianAlbert.FinanceObserver.Server.Startup.ServiceInstallers;

internal class RegistrationInstaller : IServiceInstaller
{
    private const string _expiredRegistrationDeletionExecutionPeriodInSecondsEnvKey =
        "FINANCE_OBSERVER_EXPIRED_REGISTRATION_DELETION_EXECUTION_PERIOD";

    private const string _expiredRegistrationDeletionExecutionPeriodInSecondsFileEnvKey =
        "FINANCE_OBSERVER_EXPIRED_REGISTRATION_DELETION_EXECUTION_PERIOD_FILE";

    private const string _expiredRegistrationDeletionExecutionPeriodInSecondsKey =
        "Registration:ExpiredRegistrationDeletionExecutionPeriod";

    public void Install(IServiceCollection services, IConfiguration configuration, ILogger logger)
    {
        logger.LogInformation("Adding registration workflow");

        string? expiredRegistrationDeletionExecutionPeriodInSecondsString = null;
        if (Environment.GetEnvironmentVariable(
                    _expiredRegistrationDeletionExecutionPeriodInSecondsFileEnvKey) is
                { } expiredRegistrationDeletionExecutionPeriodInSecondsFileLocation
            && File.Exists(expiredRegistrationDeletionExecutionPeriodInSecondsFileLocation))
        {
            expiredRegistrationDeletionExecutionPeriodInSecondsString =
                File.ReadAllText(expiredRegistrationDeletionExecutionPeriodInSecondsFileLocation);
        }

        expiredRegistrationDeletionExecutionPeriodInSecondsString ??=
            Environment.GetEnvironmentVariable(_expiredRegistrationDeletionExecutionPeriodInSecondsEnvKey)
            ?? configuration[_expiredRegistrationDeletionExecutionPeriodInSecondsKey];

        ArgumentException.ThrowIfNullOrEmpty(expiredRegistrationDeletionExecutionPeriodInSecondsString);
        if (!int.TryParse(expiredRegistrationDeletionExecutionPeriodInSecondsString,
                out var expiredRegistrationDeletionExecutionPeriodInSeconds))
        {
            throw new StartupValidationException(
                "There was no valid expiredRegistrationDeletion execution period found in the configuration.");
        }

        services.AddTransient<IRegistrationWorkflow, RegistrationWorkflow>();

        services.AddHostedService<ExpiredRegistrationsUserDeletionService>(serviceProvider =>
            new ExpiredRegistrationsUserDeletionService(serviceProvider,
                expiredRegistrationDeletionExecutionPeriodInSeconds));
    }
}