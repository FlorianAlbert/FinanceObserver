using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.Model;
using FlorianAlbert.FinanceObserver.Server.CrossCutting.Infrastructure;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Data;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.RegistrationConfirmationManagement.Contract;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.RegistrationConfirmationManagement;

public class RegistrationConfirmationManager : IRegistrationConfirmationManager
{
    private readonly IRepository<Guid, RegistrationConfirmation> _repository;

    public RegistrationConfirmationManager(IRepositoryFactory repositoryFactory)
    {
        _repository = repositoryFactory.CreateRepository<Guid, RegistrationConfirmation>();
    }

    public async Task<Result<RegistrationConfirmation>> RegisterAsync(RegistrationConfirmation registration,
        CancellationToken cancellationToken = default)
    {
        RegistrationConfirmation[] existingRegistrationCandidates =
        [
            .. (await _repository.QueryAsync(
            [
                Inclusion<Guid, RegistrationConfirmation>.Of<Guid, User>(existingRegistration =>
                    existingRegistration.User)
            ], cancellationToken))
            .Where(existingRegistration => existingRegistration.User == registration.User)
        ];

        if (existingRegistrationCandidates.Length > 0)
        {
            return existingRegistrationCandidates.Length switch
            {
                1 => Result.Success(existingRegistrationCandidates[0]),
                _ => Result.Failure<RegistrationConfirmation>(Errors.MultipleRegistrationConfigurationsFoundError)
            };
        }

        RegistrationConfirmation newRegistration = await _repository.InsertAsync(registration, cancellationToken);

        return Result.Success(newRegistration);
    }

    public async Task<Result> ConfirmAsync(Guid registrationConfirmationId,
        CancellationToken cancellationToken = default)
    {
        Result<RegistrationConfirmation> registrationConfirmationResult = await _repository.FindAsync(registrationConfirmationId, cancellationToken: cancellationToken);

        if (registrationConfirmationResult.Failed)
        {
            return Result.Failure(registrationConfirmationResult.Errors);
        }

        return await ConfirmAsync(registrationConfirmationResult.Value, cancellationToken);
    }

    public async Task<Result> ConfirmAsync(RegistrationConfirmation registrationConfirmation,
        CancellationToken cancellationToken = default)
    {
        if (registrationConfirmation.RegistrationIsConfirmed)
        {
            return Result.Success();
        }

        await _repository.UpdateAsync(registrationConfirmation, [Update<RegistrationConfirmation>.With(r => r.ConfirmationDate, DateTimeOffset.UtcNow)], cancellationToken);

        return Result.Success();
    }

    public async Task<Result<RegistrationConfirmation>> GetRegistrationConfirmationWithUserAsync(
        Guid registrationConfirmationId,
        CancellationToken cancellationToken = default)
    {
        RegistrationConfirmation[] existingRegistrationCandidates =
        [
            ..(await _repository.QueryAsync([Inclusion<Guid, RegistrationConfirmation>.Of<Guid, User>(r => r.User)],
                cancellationToken))
            .Where(r => r.Id == registrationConfirmationId)
        ];

        return existingRegistrationCandidates.Length switch
        {
            0 => Result.Failure<RegistrationConfirmation>(Errors.NoRegistrationConfigurationsFoundError),
            1 => Result.Success(existingRegistrationCandidates[0]),
            _ => Result.Failure<RegistrationConfirmation>(Errors.MultipleRegistrationConfigurationsFoundError)
        };
    }

    public async Task<Result<IQueryable<RegistrationConfirmation>>>
        GetUnconfirmedRegistrationConfirmationsWithUserAsync(CancellationToken cancellationToken = default)
    {
        IQueryable<RegistrationConfirmation> unconfirmedRegistrations = (await _repository.QueryAsync(
            [Inclusion<Guid, RegistrationConfirmation>.Of<Guid, User>(r => r.User)],
            cancellationToken)).Where(r => r.ConfirmationDate == null);

        return Result.Success(unconfirmedRegistrations);
    }
}