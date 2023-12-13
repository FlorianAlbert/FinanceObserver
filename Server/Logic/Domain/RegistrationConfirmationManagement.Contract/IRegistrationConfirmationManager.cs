using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.InfrastructureTypes;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.RegistrationConfirmationManagement.Contract;

public interface IRegistrationConfirmationManager
{
    Task<Result<RegistrationConfirmation>> RegisterAsync(RegistrationConfirmation registration,
        CancellationToken cancellationToken = default);

    Task<Result> ConfirmAsync(Guid registrationConfirmationId,
        CancellationToken cancellationToken = default);

    Task<Result> ConfirmAsync(RegistrationConfirmation registrationConfirmation,
        CancellationToken cancellationToken = default);

    Task<Result<RegistrationConfirmation>> GetRegistrationConfirmationWithUserAsync(Guid registrationConfirmationId,
        CancellationToken cancellationToken = default);

    Task<Result<IQueryable<RegistrationConfirmation>>> GetUnconfirmedRegistrationConfirmationsWithUserAsync(
        CancellationToken cancellationToken = default);
}