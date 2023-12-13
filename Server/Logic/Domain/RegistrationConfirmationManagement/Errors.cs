using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.InfrastructureTypes;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.RegistrationConfirmationManagement;

internal static class Errors
{
    private static Error? _multipleRegistrationConfigurationsFoundError;
    public static Error MultipleRegistrationConfigurationsFoundError =>
        _multipleRegistrationConfigurationsFoundError ??= new Error(nameof(MultipleRegistrationConfigurationsFoundError),
            "Multiple registrations were found",
            "There were multiple registrations found!",
            409);
    
    private static Error? _noRegistrationConfigurationsFoundError;
    public static Error NoRegistrationConfigurationsFoundError =>
        _noRegistrationConfigurationsFoundError ??= new Error(nameof(NoRegistrationConfigurationsFoundError),
            "No registrations were found",
            "There were no registrations found!",
            404);
}