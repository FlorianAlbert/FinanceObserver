namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.HashHandling.Contract;

public interface IHashGenerator
{
    Task<string> GenerateAsync(string input, CancellationToken cancellationToken = default);
}