using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.InfrastructureTypes;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.UserManagement;

internal static class Errors
{
    private static Error? _userAlreadyExistsError;

    public static Error UserAlreadyExistsError =>
        _userAlreadyExistsError ??= new Error(nameof(UserAlreadyExistsError),
            "User already exists",
            "The requested user could not be created because it already exists!",
            409);
}