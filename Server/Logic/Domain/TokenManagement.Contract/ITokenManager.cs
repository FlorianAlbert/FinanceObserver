using FlorianAlbert.FinanceObserver.Server.CrossCutting.Infrastructure;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.TokenManagement.Contract.Data;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.TokenManagement;

public interface ITokenManager
{
    Task<Result<TokenGenerationPayload>> GenerateTokens(TokenGenerationRequest request, CancellationToken cancellationToken = default);
}
