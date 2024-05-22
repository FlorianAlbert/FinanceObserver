using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.Model;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.TokenManagement.Contract.Data;

public class TokenGenerationRequest
{
    public required User User { get; init; }
}
