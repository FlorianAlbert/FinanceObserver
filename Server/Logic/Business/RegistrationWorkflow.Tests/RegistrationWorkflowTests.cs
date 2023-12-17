using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.InfrastructureTypes;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;
using FlorianAlbert.FinanceObserver.Server.Logic.Business.RegistrationWorkflow.Contract.Data;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.DataTransactionHandling.Contract;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.EmailManagement.Contract;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.HashHandling.Contract;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.RegistrationConfirmationManagement.Contract;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.UserManagement.Contract;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Business.RegistrationWorkflow.Tests;

public class RegistrationWorkflowTests
{
    private readonly Fixture _fixture;

    private readonly IDataTransactionHandler _dataTransactionHandlerMock;
    private readonly IUserManager _userManagerMock;
    private readonly IHashGenerator _hashGeneratorMock;
    private readonly IEmailManager _emailManagerMock;
    private readonly IRegistrationConfirmationManager _registrationConfirmationManagerMock;

    private readonly RegistrationWorkflow _sut;

    public RegistrationWorkflowTests()
    {
        _fixture = new Fixture();
        _fixture.Register(() => DateOnly.FromDateTime(_fixture.Create<DateTime>()));
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _dataTransactionHandlerMock = Substitute.For<IDataTransactionHandler>();
        _userManagerMock = Substitute.For<IUserManager>();
        _hashGeneratorMock = Substitute.For<IHashGenerator>();
        _emailManagerMock = Substitute.For<IEmailManager>();
        _registrationConfirmationManagerMock = Substitute.For<IRegistrationConfirmationManager>();

        _sut = new RegistrationWorkflow(_dataTransactionHandlerMock, _userManagerMock, _hashGeneratorMock,
            _emailManagerMock, _registrationConfirmationManagerMock);
    }

    [Fact]
    public async Task ExecuteRegistrationAsync_CallWithSuccessfulSubCalls_Succeeds()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();
        var newlyCreatedUser = _fixture.Create<User>();
        var newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(newlyCreatedRegistrationConfirmation));

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(newlyCreatedUser)),
                Arg.Any<CancellationToken>()).Returns(Result.Success());

        // Act
        var result = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithSuccessfulSubCalls_DataTransactionHandlerStartDbTransactionCalled()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();
        var newlyCreatedUser = _fixture.Create<User>();
        var newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(newlyCreatedRegistrationConfirmation));

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(newlyCreatedUser)),
                Arg.Any<CancellationToken>()).Returns(Result.Success());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _dataTransactionHandlerMock.Received(1).StartDbTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteRegistrationAsync_CallWithSuccessfulSubCalls_HashGeneratorGenerateCalled()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();
        var newlyCreatedUser = _fixture.Create<User>();
        var newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(newlyCreatedRegistrationConfirmation));

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(newlyCreatedUser)),
                Arg.Any<CancellationToken>()).Returns(Result.Success());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _hashGeneratorMock.Received(1).GenerateAsync(registrationRequest.Password, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteRegistrationAsync_CallWithSuccessfulSubCalls_UserManagerAddCalled()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();
        var newlyCreatedUser = _fixture.Create<User>();
        var newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(newlyCreatedRegistrationConfirmation));

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(newlyCreatedUser)),
                Arg.Any<CancellationToken>()).Returns(Result.Success());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _userManagerMock.Received(1).AddNewUserAsync(
            Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithSuccessfulSubCalls_RegistrationConfirmationManagerRegisterCalled()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();
        var newlyCreatedUser = _fixture.Create<User>();
        var newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(newlyCreatedRegistrationConfirmation));

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(newlyCreatedUser)),
                Arg.Any<CancellationToken>()).Returns(Result.Success());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _registrationConfirmationManagerMock.Received(1).RegisterAsync(Arg.Is<RegistrationConfirmation>(
            registrationConfirmation =>
                registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithSuccessfulSubCalls_EmailManagerSendEmailCalled()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();
        var newlyCreatedUser = _fixture.Create<User>();
        var newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(newlyCreatedRegistrationConfirmation));

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(newlyCreatedUser)),
                Arg.Any<CancellationToken>()).Returns(Result.Success());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _emailManagerMock.Received(1)
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(newlyCreatedUser)),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithSuccessfulSubCalls_DataTransactionHandlerCommitDbTransactionCalled()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();
        var newlyCreatedUser = _fixture.Create<User>();
        var newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(newlyCreatedRegistrationConfirmation));

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(newlyCreatedUser)),
                Arg.Any<CancellationToken>()).Returns(Result.Success());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _dataTransactionHandlerMock.Received(1).CommitDbTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithSuccessfulSubCalls_DataTransactionHandlerStartDbTransactionCalledBeforeAllOthers()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();
        var newlyCreatedUser = _fixture.Create<User>();
        var newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(newlyCreatedRegistrationConfirmation));

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(newlyCreatedUser)),
                Arg.Any<CancellationToken>()).Returns(Result.Success());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        Received.InOrder(() =>
        {
            _dataTransactionHandlerMock.StartDbTransactionAsync(Arg.Any<CancellationToken>());
            _hashGeneratorMock.GenerateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        });
        
        Received.InOrder(() =>
        {
            _dataTransactionHandlerMock.StartDbTransactionAsync(Arg.Any<CancellationToken>());
            _userManagerMock.AddNewUserAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        });
        
        Received.InOrder(() =>
        {
            _dataTransactionHandlerMock.StartDbTransactionAsync(Arg.Any<CancellationToken>());
            _registrationConfirmationManagerMock.RegisterAsync(Arg.Any<RegistrationConfirmation>(), Arg.Any<CancellationToken>());
        });
        
        Received.InOrder(() =>
        {
            _dataTransactionHandlerMock.StartDbTransactionAsync(Arg.Any<CancellationToken>());
            _emailManagerMock.SendEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>());
        });
        
        Received.InOrder(() =>
        {
            _dataTransactionHandlerMock.StartDbTransactionAsync(Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.CommitDbTransactionAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithSuccessfulSubCalls_DataTransactionHandlerCommitDbTransactionCalledAfterAllOthers()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();
        var newlyCreatedUser = _fixture.Create<User>();
        var newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(newlyCreatedRegistrationConfirmation));

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(newlyCreatedUser)),
                Arg.Any<CancellationToken>()).Returns(Result.Success());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        Received.InOrder(() =>
        {
            _dataTransactionHandlerMock.StartDbTransactionAsync(Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.CommitDbTransactionAsync(Arg.Any<CancellationToken>());
        });
        
        Received.InOrder(() =>
        {
            _hashGeneratorMock.GenerateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.CommitDbTransactionAsync(Arg.Any<CancellationToken>());
        });
        
        Received.InOrder(() =>
        {
            _userManagerMock.AddNewUserAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.CommitDbTransactionAsync(Arg.Any<CancellationToken>());
        });
        
        Received.InOrder(() =>
        {
            _registrationConfirmationManagerMock.RegisterAsync(Arg.Any<RegistrationConfirmation>(), Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.CommitDbTransactionAsync(Arg.Any<CancellationToken>());
        });
        
        Received.InOrder(() =>
        {
            _emailManagerMock.SendEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.CommitDbTransactionAsync(Arg.Any<CancellationToken>());
        });
    }
}