using FlorianAlbert.FinanceObserver.Server.CrossCutting.Infrastructure;
using FlorianAlbert.FinanceObserver.Server.Logic.Business.RegistrationWorkflow.Contract.Data;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Business.RegistrationWorkflow.Contract;

public interface IRegistrationWorkflow
{
    Task<Result> ExecuteRegistrationAsync(RegistrationWorkflowRequest registrationWorkflowRequest,
        CancellationToken cancellationToken = default);

    Task<Result> ExecuteConfirmationAsync(ConfirmationWorkflowRequest confirmationWorkflowRequest,
        CancellationToken cancellationToken = default);
}