using FlorianAlbert.FinanceObserver.Server.CrossCutting.Infrastructure;
using FlorianAlbert.FinanceObserver.Server.Logic.Business.AuthorizationWorkflow.Contract.Data;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Business.AuthorizationWorkflow.Contract;

public interface IAuthorizationWorkflow
{
    Task<Result<AuthorizationPayload>> ExecuteAuthorizationAsync(AuthorizationWorkflowRequest authorizationWorkflowRequest,
        CancellationToken cancellationToken = default);
}