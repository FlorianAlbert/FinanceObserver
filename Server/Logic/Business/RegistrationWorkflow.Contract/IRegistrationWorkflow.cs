using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.InfrastructureTypes;
using FlorianAlbert.FinanceObserver.Server.Logic.Business.RegistrationWorkflow.Contract.Data;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Business.RegistrationWorkflow.Contract;

public interface IRegistrationWorkflow
{
    Task<Result> ExecuteRegistrationAsync(RegistrationRequest registrationRequest,
        CancellationToken cancellationToken = default);

    Task<Result> ExecuteConfirmationAsync(ConfirmationRequest confirmationRequest,
        CancellationToken cancellationToken = default);
}