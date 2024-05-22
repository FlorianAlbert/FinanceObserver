namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.TokenManagement.Contract.Data;

public class TokenGenerationPayload
{
    public required string AccessToken { get; init; }

    public required string RefreshToken { get; init; }

    public required DateTimeOffset AccessTokenExpiration { get; init; }
}
