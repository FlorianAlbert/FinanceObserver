namespace FlorianAlbert.FinanceObserver.Server.Logic.Business.AuthorizationWorkflow.Contract.Data;

public class AuthorizationPayload
{
    public required string AccessToken { get; init; }

    public required string RefreshToken { get; init; }

    public required DateTimeOffset AccessTokenExpiration { get; init; }
}