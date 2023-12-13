namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.HashHandling.Contract;

public interface IHashValidator
{
    Task<bool> ValidateAsync(string validated, string savedHash, CancellationToken cancellationToken = default);
}