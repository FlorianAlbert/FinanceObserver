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
        ExecuteRegistrationAsync_CallWithSuccessfulSubCalls_DataTransactionHandlerRollbackDbTransactionNotCalled()
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
        await _dataTransactionHandlerMock.DidNotReceive().RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
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
            _registrationConfirmationManagerMock.RegisterAsync(Arg.Any<RegistrationConfirmation>(),
                Arg.Any<CancellationToken>());
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
            _registrationConfirmationManagerMock.RegisterAsync(Arg.Any<RegistrationConfirmation>(),
                Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.CommitDbTransactionAsync(Arg.Any<CancellationToken>());
        });

        Received.InOrder(() =>
        {
            _emailManagerMock.SendEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.CommitDbTransactionAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task ExecuteRegistrationAsync_CallWithFailingUserAddition_Fails()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Failure());

        // Act
        var result = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        result.Failed.Should().BeTrue();
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithFailingUserAddition_DataTransactionHandlerStartDbTransactionCalled()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Failure());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _dataTransactionHandlerMock.Received(1).StartDbTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteRegistrationAsync_CallWithFailingUserAddition_HashGeneratorGenerateCalled()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Failure());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _hashGeneratorMock.Received(1).GenerateAsync(registrationRequest.Password, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteRegistrationAsync_CallWithFailingUserAddition_UserManagerAddCalled()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Failure());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _userManagerMock.Received(1).AddNewUserAsync(
            Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithFailingUserAddition_RegistrationConfirmationManagerRegisterNotCalled()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Failure());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _registrationConfirmationManagerMock.DidNotReceive()
            .RegisterAsync(Arg.Any<RegistrationConfirmation>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithFailingUserAddition_EmailManagerSendEmailNotCalled()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Failure());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _emailManagerMock.DidNotReceive()
            .SendEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithFailingUserAddition_DataTransactionHandlerCommitDbTransactionNotCalled()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Failure());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _dataTransactionHandlerMock.DidNotReceive().CommitDbTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithFailingUserAddition_DataTransactionHandlerRollbackDbTransactionCalled()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Failure());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _dataTransactionHandlerMock.Received(1).RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithFailingUserAddition_DataTransactionHandlerStartDbTransactionCalledBeforeAllOthers()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Failure());

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
            _dataTransactionHandlerMock.RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithFailingUserAddition_DataTransactionHandlerRollbackDbTransactionCalledAfterAllOthers()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Failure());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        Received.InOrder(() =>
        {
            _dataTransactionHandlerMock.StartDbTransactionAsync(Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
        });

        Received.InOrder(() =>
        {
            _hashGeneratorMock.GenerateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
        });

        Received.InOrder(() =>
        {
            _userManagerMock.AddNewUserAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task ExecuteRegistrationAsync_CallWithFailingRegistrationConfirmationAddition_Fails()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();
        var newlyCreatedUser = _fixture.Create<User>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Failure());

        // Act
        var result = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        result.Failed.Should().BeTrue();
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithFailingRegistrationConfirmationAddition_DataTransactionHandlerStartDbTransactionCalled()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();
        var newlyCreatedUser = _fixture.Create<User>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Failure());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _dataTransactionHandlerMock.Received(1).StartDbTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithFailingRegistrationConfirmationAddition_HashGeneratorGenerateCalled()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();
        var newlyCreatedUser = _fixture.Create<User>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Failure());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _hashGeneratorMock.Received(1).GenerateAsync(registrationRequest.Password, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteRegistrationAsync_CallWithFailingRegistrationConfirmationAddition_UserManagerAddCalled()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();
        var newlyCreatedUser = _fixture.Create<User>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Failure());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _userManagerMock.Received(1).AddNewUserAsync(
            Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithFailingRegistrationConfirmationAddition_RegistrationConfirmationManagerRegisterCalled()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();
        var newlyCreatedUser = _fixture.Create<User>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Failure());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _registrationConfirmationManagerMock.Received(1).RegisterAsync(Arg.Is<RegistrationConfirmation>(
            registrationConfirmation =>
                registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithFailingRegistrationConfirmationAddition_EmailManagerSendEmailNotCalled()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();
        var newlyCreatedUser = _fixture.Create<User>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Failure());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _emailManagerMock.DidNotReceive()
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(newlyCreatedUser)),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithFailingRegistrationConfirmationAddition_DataTransactionHandlerCommitDbTransactionNotCalled()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();
        var newlyCreatedUser = _fixture.Create<User>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Failure());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _dataTransactionHandlerMock.DidNotReceive().CommitDbTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithFailingRegistrationConfirmationAddition_DataTransactionHandlerRollbackDbTransactionCalled()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();
        var newlyCreatedUser = _fixture.Create<User>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Failure());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _dataTransactionHandlerMock.Received(1).RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithFailingRegistrationConfirmationAddition_DataTransactionHandlerStartDbTransactionCalledBeforeAllOthers()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();
        var newlyCreatedUser = _fixture.Create<User>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Failure());

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
            _registrationConfirmationManagerMock.RegisterAsync(Arg.Any<RegistrationConfirmation>(),
                Arg.Any<CancellationToken>());
        });

        Received.InOrder(() =>
        {
            _dataTransactionHandlerMock.StartDbTransactionAsync(Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithFailingRegistrationConfirmationAddition_DataTransactionHandlerRollbackDbTransactionCalledAfterAllOthers()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();
        var newlyCreatedUser = _fixture.Create<User>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result<User>.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Failure());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        Received.InOrder(() =>
        {
            _dataTransactionHandlerMock.StartDbTransactionAsync(Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
        });

        Received.InOrder(() =>
        {
            _hashGeneratorMock.GenerateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
        });

        Received.InOrder(() =>
        {
            _userManagerMock.AddNewUserAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
        });

        Received.InOrder(() =>
        {
            _registrationConfirmationManagerMock.RegisterAsync(Arg.Any<RegistrationConfirmation>(),
                Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task ExecuteRegistrationAsync_CallWithFailingEmailSending_Fails()
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
                Arg.Any<CancellationToken>()).Returns(Result.Failure());

        // Act
        var result = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        result.Failed.Should().BeTrue();
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithFailingEmailSending_DataTransactionHandlerStartDbTransactionCalled()
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
                Arg.Any<CancellationToken>()).Returns(Result.Failure());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _dataTransactionHandlerMock.Received(1).StartDbTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteRegistrationAsync_CallWithFailingEmailSending_HashGeneratorGenerateCalled()
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
                Arg.Any<CancellationToken>()).Returns(Result.Failure());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _hashGeneratorMock.Received(1).GenerateAsync(registrationRequest.Password, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteRegistrationAsync_CallWithFailingEmailSending_UserManagerAddCalled()
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
                Arg.Any<CancellationToken>()).Returns(Result.Failure());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _userManagerMock.Received(1).AddNewUserAsync(
            Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithFailingEmailSending_RegistrationConfirmationManagerRegisterCalled()
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
                Arg.Any<CancellationToken>()).Returns(Result.Failure());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _registrationConfirmationManagerMock.Received(1).RegisterAsync(Arg.Is<RegistrationConfirmation>(
            registrationConfirmation =>
                registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithFailingEmailSending_EmailManagerSendEmailCalled()
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
                Arg.Any<CancellationToken>()).Returns(Result.Failure());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _emailManagerMock.Received(1)
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(newlyCreatedUser)),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithFailingEmailSending_DataTransactionHandlerRollbackDbTransactionCalled()
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
                Arg.Any<CancellationToken>()).Returns(Result.Failure());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _dataTransactionHandlerMock.Received(1).RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsyncCallWithFailingEmailSending_DataTransactionHandlerCommitDbTransactionNotCalled()
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
                Arg.Any<CancellationToken>()).Returns(Result.Failure());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _dataTransactionHandlerMock.DidNotReceive().CommitDbTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithFailingEmailSending_DataTransactionHandlerStartDbTransactionCalledBeforeAllOthers()
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
                Arg.Any<CancellationToken>()).Returns(Result.Failure());

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
            _registrationConfirmationManagerMock.RegisterAsync(Arg.Any<RegistrationConfirmation>(),
                Arg.Any<CancellationToken>());
        });

        Received.InOrder(() =>
        {
            _dataTransactionHandlerMock.StartDbTransactionAsync(Arg.Any<CancellationToken>());
            _emailManagerMock.SendEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>());
        });

        Received.InOrder(() =>
        {
            _dataTransactionHandlerMock.StartDbTransactionAsync(Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithFailingEmailSending_DataTransactionHandlerRollbackDbTransactionCalledAfterAllOthers()
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
                Arg.Any<CancellationToken>()).Returns(Result.Failure());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        Received.InOrder(() =>
        {
            _dataTransactionHandlerMock.StartDbTransactionAsync(Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
        });

        Received.InOrder(() =>
        {
            _hashGeneratorMock.GenerateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
        });

        Received.InOrder(() =>
        {
            _userManagerMock.AddNewUserAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
        });

        Received.InOrder(() =>
        {
            _registrationConfirmationManagerMock.RegisterAsync(Arg.Any<RegistrationConfirmation>(),
                Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
        });

        Received.InOrder(() =>
        {
            _emailManagerMock.SendEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task ExecuteConfirmationAsync_CallWithExistingRegistrationConfirmationAndSuccessfulSubCalls_Succeeds()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();
        var existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(existingRegistrationConfirmation));

        _registrationConfirmationManagerMock
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>()).Returns(Result.Success());

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(existingRegistrationConfirmation.User)),
                Arg.Any<CancellationToken>()).Returns(Result.Success());

        // Act
        var result = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithExistingRegistrationConfirmationAndSuccessfulSubCalls_DataTransactionHandlerStartDbTransactionCalled()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();
        var existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(existingRegistrationConfirmation));

        _registrationConfirmationManagerMock
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>()).Returns(Result.Success());

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(existingRegistrationConfirmation.User)),
                Arg.Any<CancellationToken>()).Returns(Result.Success());

        // Act
        _ = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        await _dataTransactionHandlerMock.Received(1).StartDbTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithExistingRegistrationConfirmationAndSuccessfulSubCalls_RegistrationConfirmationManagerConfirmCalled()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();
        var existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(existingRegistrationConfirmation));

        _registrationConfirmationManagerMock
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>()).Returns(Result.Success());

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(existingRegistrationConfirmation.User)),
                Arg.Any<CancellationToken>()).Returns(Result.Success());

        // Act
        _ = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        await _registrationConfirmationManagerMock.Received(1)
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithExistingRegistrationConfirmationAndSuccessfulSubCalls_EmailManagerSendEmailCalled()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();
        var existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(existingRegistrationConfirmation));

        _registrationConfirmationManagerMock
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>()).Returns(Result.Success());

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(existingRegistrationConfirmation.User)),
                Arg.Any<CancellationToken>()).Returns(Result.Success());

        // Act
        _ = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        await _emailManagerMock.Received(1)
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(existingRegistrationConfirmation.User)),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithExistingRegistrationConfirmationAndSuccessfulSubCalls_DataTransactionHandlerCommitDbTransactionCalled()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();
        var existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(existingRegistrationConfirmation));

        _registrationConfirmationManagerMock
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>()).Returns(Result.Success());

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(existingRegistrationConfirmation.User)),
                Arg.Any<CancellationToken>()).Returns(Result.Success());

        // Act
        _ = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        await _dataTransactionHandlerMock.Received(1).CommitDbTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithExistingRegistrationConfirmationAndSuccessfulSubCalls_DataTransactionHandlerRollbackDbTransactionNotCalled()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();
        var existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(existingRegistrationConfirmation));

        _registrationConfirmationManagerMock
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>()).Returns(Result.Success());

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(existingRegistrationConfirmation.User)),
                Arg.Any<CancellationToken>()).Returns(Result.Success());

        // Act
        _ = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        await _dataTransactionHandlerMock.DidNotReceive().RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithExistingRegistrationConfirmationAndSuccessfulSubCalls_DataTransactionHandlerStartDbTransactionCalledBeforeAllOthers()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();
        var existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(existingRegistrationConfirmation));

        _registrationConfirmationManagerMock
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>()).Returns(Result.Success());

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(existingRegistrationConfirmation.User)),
                Arg.Any<CancellationToken>()).Returns(Result.Success());

        // Act
        _ = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        Received.InOrder(() =>
        {
            _dataTransactionHandlerMock.StartDbTransactionAsync(Arg.Any<CancellationToken>());
            _registrationConfirmationManagerMock.ConfirmAsync(Arg.Any<RegistrationConfirmation>(),
                Arg.Any<CancellationToken>());
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
        ExecuteConfirmationAsync_CallWithExistingRegistrationConfirmationAndSuccessfulSubCalls_DataTransactionHandlerCommitDbTransactionCalledAfterAllOthers()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();
        var existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(existingRegistrationConfirmation));

        _registrationConfirmationManagerMock
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>()).Returns(Result.Success());

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(existingRegistrationConfirmation.User)),
                Arg.Any<CancellationToken>()).Returns(Result.Success());

        // Act
        _ = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        Received.InOrder(() =>
        {
            _dataTransactionHandlerMock.StartDbTransactionAsync(Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.CommitDbTransactionAsync(Arg.Any<CancellationToken>());
        });

        Received.InOrder(() =>
        {
            _registrationConfirmationManagerMock.ConfirmAsync(Arg.Any<RegistrationConfirmation>(),
                Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.CommitDbTransactionAsync(Arg.Any<CancellationToken>());
        });

        Received.InOrder(() =>
        {
            _emailManagerMock.SendEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.CommitDbTransactionAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task ExecuteConfirmationAsync_CallWithFailingRegistrationConfirmationQueryCall_Fails()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Failure());

        // Act
        var result = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        result.Failed.Should().BeTrue();
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithFailingRegistrationConfirmationQueryCall_DataTransactionHandlerStartDbTransactionNotCalled()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Failure());

        // Act
        _ = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        await _dataTransactionHandlerMock.DidNotReceive().StartDbTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithFailingRegistrationConfirmationQueryCall_RegistrationConfirmationManagerConfirmNotCalled()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Failure());

        // Act
        _ = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        await _registrationConfirmationManagerMock.DidNotReceive()
            .ConfirmAsync(Arg.Any<RegistrationConfirmation>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithFailingRegistrationConfirmationQueryCall_EmailManagerSendEmailNotCalled()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Failure());

        // Act
        _ = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        await _emailManagerMock.DidNotReceive().SendEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithFailingRegistrationConfirmationQueryCall_DataTransactionHandlerCommitDbTransactionNotCalled()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Failure());

        // Act
        _ = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        await _dataTransactionHandlerMock.DidNotReceive().CommitDbTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithFailingRegistrationConfirmationQueryCall_DataTransactionHandlerRollbackDbTransactionNotCalled()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Failure());

        // Act
        _ = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        await _dataTransactionHandlerMock.DidNotReceive().RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteConfirmationAsync_CallWithFailingConfirmation_Fails()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();
        var existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(existingRegistrationConfirmation));

        _registrationConfirmationManagerMock
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>()).Returns(Result.Failure());

        // Act
        var result = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        result.Failed.Should().BeTrue();
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithFailingConfirmation_DataTransactionHandlerStartDbTransactionCalled()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();
        var existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(existingRegistrationConfirmation));

        _registrationConfirmationManagerMock
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>()).Returns(Result.Failure());

        // Act
        _ = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        await _dataTransactionHandlerMock.Received(1).StartDbTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithFailingConfirmation_RegistrationConfirmationManagerConfirmCalled()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();
        var existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(existingRegistrationConfirmation));

        _registrationConfirmationManagerMock
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>()).Returns(Result.Failure());

        // Act
        _ = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        await _registrationConfirmationManagerMock.Received(1)
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithFailingConfirmation_EmailManagerSendEmailNotCalled()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();
        var existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(existingRegistrationConfirmation));

        _registrationConfirmationManagerMock
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>()).Returns(Result.Failure());

        // Act
        _ = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        await _emailManagerMock.DidNotReceive().SendEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithFailingConfirmation_DataTransactionHandlerCommitDbTransactionNotCalled()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();
        var existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(existingRegistrationConfirmation));

        _registrationConfirmationManagerMock
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>()).Returns(Result.Failure());

        // Act
        _ = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        await _dataTransactionHandlerMock.DidNotReceive().CommitDbTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithFailingConfirmation_DataTransactionHandlerRollbackDbTransactionCalled()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();
        var existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(existingRegistrationConfirmation));

        _registrationConfirmationManagerMock
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>()).Returns(Result.Failure());

        // Act
        _ = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        await _dataTransactionHandlerMock.Received(1).RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithFailingConfirmation_DataTransactionHandlerStartDbTransactionCalledBeforeAllOthers()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();
        var existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(existingRegistrationConfirmation));

        _registrationConfirmationManagerMock
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>()).Returns(Result.Failure());

        // Act
        _ = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        Received.InOrder(() =>
        {
            _dataTransactionHandlerMock.StartDbTransactionAsync(Arg.Any<CancellationToken>());
            _registrationConfirmationManagerMock.ConfirmAsync(Arg.Any<RegistrationConfirmation>(),
                Arg.Any<CancellationToken>());
        });

        Received.InOrder(() =>
        {
            _dataTransactionHandlerMock.StartDbTransactionAsync(Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithFailingConfirmation_DataTransactionHandlerRollbackDbTransactionCalledAfterAllOthers()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();
        var existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(existingRegistrationConfirmation));

        _registrationConfirmationManagerMock
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>()).Returns(Result.Failure());

        // Act
        _ = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        Received.InOrder(() =>
        {
            _dataTransactionHandlerMock.StartDbTransactionAsync(Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
        });

        Received.InOrder(() =>
        {
            _registrationConfirmationManagerMock.ConfirmAsync(Arg.Any<RegistrationConfirmation>(),
                Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task ExecuteConfirmationAsync_CallWithFailingEmailSending_Fails()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();
        var existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(existingRegistrationConfirmation));

        _registrationConfirmationManagerMock
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>()).Returns(Result.Success());

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(existingRegistrationConfirmation.User)),
                Arg.Any<CancellationToken>()).Returns(Result.Failure());

        // Act
        var result = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        result.Failed.Should().BeTrue();
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithFailingEmailSending_DataTransactionHandlerStartDbTransactionCalled()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();
        var existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(existingRegistrationConfirmation));

        _registrationConfirmationManagerMock
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>()).Returns(Result.Success());

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(existingRegistrationConfirmation.User)),
                Arg.Any<CancellationToken>()).Returns(Result.Failure());

        // Act
        _ = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        await _dataTransactionHandlerMock.Received(1).StartDbTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithFailingEmailSending_RegistrationConfirmationManagerConfirmCalled()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();
        var existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(existingRegistrationConfirmation));

        _registrationConfirmationManagerMock
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>()).Returns(Result.Success());

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(existingRegistrationConfirmation.User)),
                Arg.Any<CancellationToken>()).Returns(Result.Failure());

        // Act
        _ = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        await _registrationConfirmationManagerMock.Received(1)
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithFailingEmailSending_EmailManagerSendEmailCalled()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();
        var existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(existingRegistrationConfirmation));

        _registrationConfirmationManagerMock
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>()).Returns(Result.Success());

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(existingRegistrationConfirmation.User)),
                Arg.Any<CancellationToken>()).Returns(Result.Failure());

        // Act
        _ = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        await _emailManagerMock.Received(1)
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(existingRegistrationConfirmation.User)),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithFailingEmailSending_DataTransactionHandlerCommitDbTransactionNotCalled()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();
        var existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(existingRegistrationConfirmation));

        _registrationConfirmationManagerMock
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>()).Returns(Result.Success());

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(existingRegistrationConfirmation.User)),
                Arg.Any<CancellationToken>()).Returns(Result.Failure());

        // Act
        _ = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        await _dataTransactionHandlerMock.DidNotReceive().CommitDbTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithFailingEmailSending_DataTransactionHandlerRollbackDbTransactionCalled()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();
        var existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(existingRegistrationConfirmation));

        _registrationConfirmationManagerMock
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>()).Returns(Result.Success());

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(existingRegistrationConfirmation.User)),
                Arg.Any<CancellationToken>()).Returns(Result.Failure());

        // Act
        _ = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        await _dataTransactionHandlerMock.Received(1).RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithFailingEmailSending_DataTransactionHandlerStartDbTransactionCalledBeforeAllOthers()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();
        var existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(existingRegistrationConfirmation));

        _registrationConfirmationManagerMock
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>()).Returns(Result.Success());

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(existingRegistrationConfirmation.User)),
                Arg.Any<CancellationToken>()).Returns(Result.Failure());

        // Act
        _ = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        Received.InOrder(() =>
        {
            _dataTransactionHandlerMock.StartDbTransactionAsync(Arg.Any<CancellationToken>());
            _registrationConfirmationManagerMock.ConfirmAsync(Arg.Any<RegistrationConfirmation>(),
                Arg.Any<CancellationToken>());
        });

        Received.InOrder(() =>
        {
            _dataTransactionHandlerMock.StartDbTransactionAsync(Arg.Any<CancellationToken>());
            _emailManagerMock.SendEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>());
        });

        Received.InOrder(() =>
        {
            _dataTransactionHandlerMock.StartDbTransactionAsync(Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithFailingEmailSending_DataTransactionHandlerRollbackDbTransactionCalledAfterAllOthers()
    {
        // Arrange
        var confirmationRequest = _fixture.Create<ConfirmationRequest>();
        var existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(existingRegistrationConfirmation));

        _registrationConfirmationManagerMock
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>()).Returns(Result.Success());

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(existingRegistrationConfirmation.User)),
                Arg.Any<CancellationToken>()).Returns(Result.Failure());

        // Act
        _ = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        Received.InOrder(() =>
        {
            _dataTransactionHandlerMock.StartDbTransactionAsync(Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
        });

        Received.InOrder(() =>
        {
            _registrationConfirmationManagerMock.ConfirmAsync(Arg.Any<RegistrationConfirmation>(),
                Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
        });

        Received.InOrder(() =>
        {
            _emailManagerMock.SendEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>());
            _dataTransactionHandlerMock.RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
        });
    }
}