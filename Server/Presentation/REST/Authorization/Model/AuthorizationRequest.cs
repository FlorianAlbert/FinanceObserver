namespace FlorianAlbert.FinanceObserver.Server.Presentation.REST.Authorization.Model;

/// <summary>
///     A request to to authorize a user
/// </summary>
public class AuthorizationRequest
{
    /// <summary>
    ///     The requested email address
    /// </summary>
    /// <example>Max.Mustermann@muster.com</example>
    public required string EmailAddress { get; init; }

    /// <summary>
    ///     The provided password the user wants to authorize with
    /// </summary>
    /// <example>Password123</example>
    public required string Password { get; init; }
}