using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.InfrastructureTypes;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Data;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;
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
                Inclusion<Guid, RegistrationConfirmation>.Of(existingRegistration =>
                    existingRegistration.User)
            ], cancellationToken))
            .Where(existingRegistration => existingRegistration.User == registration.User)
        ];

        if (existingRegistrationCandidates.Length > 0)
        {
            return existingRegistrationCandidates.Length switch
            {
                1 => Result<RegistrationConfirmation>.Success(existingRegistrationCandidates[0]),
                _ => Result<RegistrationConfirmation>.Failure(Errors.MultipleRegistrationConfigurationsFoundError)
            };
        }

        var newRegistration = await _repository.InsertAsync(registration, cancellationToken);

        return Result<RegistrationConfirmation>.Success(newRegistration);
    }

    public async Task<Result> ConfirmAsync(Guid registrationConfirmationId,
        CancellationToken cancellationToken = default)
    {
        var registrationConfirmationResult = await _repository.FindAsync(registrationConfirmationId, cancellationToken);

        if (registrationConfirmationResult.Failed)
        {
            return Result.Failure(registrationConfirmationResult.Errors);
        }

        return await ConfirmAsync(registrationConfirmationResult.Value!, cancellationToken);
    }

    public async Task<Result> ConfirmAsync(RegistrationConfirmation registrationConfirmation,
        CancellationToken cancellationToken = default)
    {
        if (registrationConfirmation.RegistrationIsConfirmed)
        {
            return Result.Success();
        }
        
        registrationConfirmation.ConfirmationDate = DateTime.UtcNow;
        
        await _repository.UpdateAsync(registrationConfirmation, cancellationToken);

        return Result.Success();
    }

    public async Task<Result<RegistrationConfirmation>> GetRegistrationConfirmationWithUserAsync(
        Guid registrationConfirmationId,
        CancellationToken cancellationToken = default)
    {
        RegistrationConfirmation[] existingRegistrationCandidates =
        [
            ..(await _repository.QueryAsync([Inclusion<Guid, RegistrationConfirmation>.Of(r => r.User)],
                cancellationToken))
            .Where(r => r.Id == registrationConfirmationId)
        ];

        return existingRegistrationCandidates.Length switch
        {
            0 => Result<RegistrationConfirmation>.Failure(Errors.NoRegistrationConfigurationsFoundError),
            1 => Result<RegistrationConfirmation>.Success(existingRegistrationCandidates[0]),
            _ => Result<RegistrationConfirmation>.Failure(Errors.MultipleRegistrationConfigurationsFoundError)
        };
    }

    public async Task<Result<IQueryable<RegistrationConfirmation>>>
        GetUnconfirmedRegistrationConfirmationsWithUserAsync(CancellationToken cancellationToken = default)
    {
        var unconfirmedRegistrations = (await _repository.QueryAsync(
            [Inclusion<Guid, RegistrationConfirmation>.Of(r => r.User)],
            cancellationToken)).Where(r => r.ConfirmationDate == null);

        return Result<IQueryable<RegistrationConfirmation>>.Success(unconfirmedRegistrations);
    }
}