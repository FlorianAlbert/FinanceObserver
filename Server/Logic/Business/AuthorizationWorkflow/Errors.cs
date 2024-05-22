using FlorianAlbert.FinanceObserver.Server.CrossCutting.Infrastructure;

namespace AuthorizationWorkflow;

internal static class Errors
{
    private static Error? _authorizationFailedError;
    internal static Error AuthorizationFailedError =>
        _authorizationFailedError ??= new Error(nameof(AuthorizationFailedError),
            "Authorization failed",
            "The authorization failed because the provided email address or credentials were incorrect!",
            400);
}