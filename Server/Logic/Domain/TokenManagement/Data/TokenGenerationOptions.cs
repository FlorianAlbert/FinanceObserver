namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.TokenManagement.Data;

public class TokenGenerationOptions
{
    public required string SignatureSecret { get; init; }

    public required double AccessTokenExpirationInMinutes { get; init; }
}
