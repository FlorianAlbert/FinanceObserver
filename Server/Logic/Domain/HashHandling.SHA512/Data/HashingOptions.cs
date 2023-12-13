namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.HashHandling.SHA512.Data;

public class HashingOptions
{
    public required int HashSize { get; init; }

    public required int SaltSize { get; init; }

    public required int Iterations { get; init; }
}