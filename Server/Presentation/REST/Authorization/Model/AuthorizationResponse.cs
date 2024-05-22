namespace FlorianAlbert.FinanceObserver.Server.Presentation.REST.Authorization.Model;

/// <summary>
///    The response to an authorization request
/// </summary>
public class AuthorizationResponse
{
    /// <summary>
    ///    The access token to authorize the user
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>
    ///   The refresh token to refresh the access token
    /// </summary>
    public required string RefreshToken { get; init; }

    /// <summary>
    ///   The expiration date of the access token
    /// </summary>
    public required DateTimeOffset AccessTokenExpiration { get; init; }
}
