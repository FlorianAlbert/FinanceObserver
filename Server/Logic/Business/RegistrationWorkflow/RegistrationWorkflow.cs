using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.Constants;
using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.InfrastructureTypes;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;
using FlorianAlbert.FinanceObserver.Server.Logic.Business.RegistrationWorkflow.Contract;
using FlorianAlbert.FinanceObserver.Server.Logic.Business.RegistrationWorkflow.Contract.Data;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.DataTransactionHandling.Contract;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.EmailManagement.Contract;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.HashHandling.Contract;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.RegistrationConfirmationManagement.Contract;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.UserManagement.Contract;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Business.RegistrationWorkflow;

public class RegistrationWorkflow : IRegistrationWorkflow
{
    private readonly IDataTransactionHandler _dataTransactionHandler;
    private readonly IEmailManager _emailManager;
    private readonly IHashGenerator _hashGenerator;
    private readonly IRegistrationConfirmationManager _registrationConfirmationManager;
    private readonly IUserManager _userManager;

    public RegistrationWorkflow(IDataTransactionHandler dataTransactionHandler, IUserManager userManager,
        IHashGenerator hashGenerator, IEmailManager emailManager,
        IRegistrationConfirmationManager registrationConfirmationManager)
    {
        _dataTransactionHandler = dataTransactionHandler;
        _userManager = userManager;
        _hashGenerator = hashGenerator;
        _emailManager = emailManager;
        _registrationConfirmationManager = registrationConfirmationManager;
    }

    public async Task<Result> ExecuteRegistrationAsync(RegistrationRequest registrationRequest,
        CancellationToken cancellationToken = default)
    {
        await _dataTransactionHandler.StartDbTransactionAsync(cancellationToken);

        // Create password hash 
        var hash = await _hashGenerator.GenerateAsync(registrationRequest.Password, cancellationToken);

        // Create new user
        var userToAdd = new User
        {
            Id = Guid.Empty,
            UserName = registrationRequest.Username,
            FirstName = registrationRequest.FirstName,
            LastName = registrationRequest.LastName,
            EmailAddress = registrationRequest.EmailAddress,
            BirthDate = registrationRequest.BirthDate,
            PasswordHash = hash
        };

        // Add new user
        var userInsertResult = await _userManager.AddNewUserAsync(userToAdd, cancellationToken);

        if (userInsertResult.Failed)
        {
            await _dataTransactionHandler.RollbackDbTransactionAsync(cancellationToken);

            return Result.Failure(userInsertResult.Errors);
        }

        var addedUser = userInsertResult.Value!;

        // Create RegistrationConfirmation
        var registrationToAdd = new RegistrationConfirmation
        {
            Id = Guid.Empty,
            User = addedUser
        };

        // Add new RegistrationConfirmation
        var registrationConfirmationInsertResult =
            await _registrationConfirmationManager.RegisterAsync(registrationToAdd, cancellationToken);

        if (registrationConfirmationInsertResult.Failed)
        {
            await _dataTransactionHandler.RollbackDbTransactionAsync(cancellationToken);

            return Result.Failure(registrationConfirmationInsertResult.Errors);
        }

        var addedRegistration = registrationConfirmationInsertResult.Value!;

        // Create registration confirmation mail
        var registrationConfirmationEmail = new Email
        {
            Id = Guid.Empty,
            Receivers = [addedUser],
            Subject = "Welcome to Observe!",
            Message = $"Hi {addedUser.FirstName}!\n" +
                      "\n" +
                      $"Welcome to Observe! We are very happy you decided to be a part of our community! {Emoji.SmilingFace}\n" +
                      "But before you can start with saving money, we would like you to confirm your email address by clicking the following link: \n" +
                      $"{registrationRequest.ConfirmationLinkTemplate.Replace("<confirmationId>", addedRegistration.Id.ToString())}\n" +
                      "\n" +
                      "See you soon!\n" +
                      "Your Observe-Team"
        };

        // Send registration confirmation mail
        var emailResult = await _emailManager.SendEmailAsync(registrationConfirmationEmail, cancellationToken);

        if (emailResult.Failed)
        {
            await _dataTransactionHandler.RollbackDbTransactionAsync(cancellationToken);

            return Result.Failure(emailResult.Errors);
        }

        await _dataTransactionHandler.CommitDbTransactionAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ExecuteConfirmationAsync(ConfirmationRequest confirmationRequest,
        CancellationToken cancellationToken = default)
    {
        var registrationConfirmationResult =
            await _registrationConfirmationManager.GetRegistrationConfirmationWithUserAsync(
                confirmationRequest.ConfirmationId, cancellationToken);

        if (registrationConfirmationResult.Failed)
        {
            return Result.Failure(registrationConfirmationResult.Errors);
        }

        var registrationConfirmation = registrationConfirmationResult.Value!;
        
        await _dataTransactionHandler.StartDbTransactionAsync(cancellationToken);

        var confirmationResult =
            await _registrationConfirmationManager.ConfirmAsync(registrationConfirmation, cancellationToken);

        if (confirmationResult.Failed)
        {
            await _dataTransactionHandler.RollbackDbTransactionAsync(cancellationToken);
            
            return Result.Failure(confirmationResult.Errors);
        }
        
        // Create confirmation mail
        var receiver = registrationConfirmation.User;
        
        var confirmationEmail = new Email
        {
            Id = Guid.Empty,
            Receivers = [receiver],
            Subject = "Your registration was successful!",
            Message = $"Hi {receiver.FirstName}!\n" +
                      "\n" +
                      $"You successfully confirmed your email address and can get fully started now! {Emoji.PartyPopper}\n" +
                      "Why don't you look around a little bit and explore all of our fantastic features?\n" +
                      "\n" +
                      "Have fun!\n" +
                      "Your Observe-Team"
        };

        // Send confirmation mail
        var emailResult = await _emailManager.SendEmailAsync(confirmationEmail, cancellationToken);

        if (emailResult.Failed)
        {
            await _dataTransactionHandler.RollbackDbTransactionAsync(cancellationToken);

            return Result.Failure(emailResult.Errors);
        }

        await _dataTransactionHandler.CommitDbTransactionAsync(cancellationToken);

        return Result.Success();
    }
}