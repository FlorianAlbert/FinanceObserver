using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.Model;
using FlorianAlbert.FinanceObserver.Server.CrossCutting.Infrastructure;
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
        _fixture.Register(() => DateOnly.FromDateTime(_fixture.Create<DateTimeOffset>().Date));
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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();
        RegistrationConfirmation newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Success(newlyCreatedRegistrationConfirmation));

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(newlyCreatedUser)),
                Arg.Any<CancellationToken>()).Returns(Result.Success());

        // Act
        Result result = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithSuccessfulSubCalls_DataTransactionHandlerStartDbTransactionCalled()
    {
        // Arrange
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();
        RegistrationConfirmation newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Success(newlyCreatedRegistrationConfirmation));

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();
        RegistrationConfirmation newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Success(newlyCreatedRegistrationConfirmation));

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();
        RegistrationConfirmation newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Success(newlyCreatedRegistrationConfirmation));

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();
        RegistrationConfirmation newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Success(newlyCreatedRegistrationConfirmation));

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();
        RegistrationConfirmation newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Success(newlyCreatedRegistrationConfirmation));

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();
        RegistrationConfirmation newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Success(newlyCreatedRegistrationConfirmation));

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();
        RegistrationConfirmation newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Success(newlyCreatedRegistrationConfirmation));

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();
        RegistrationConfirmation newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Success(newlyCreatedRegistrationConfirmation));

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();
        RegistrationConfirmation newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Success(newlyCreatedRegistrationConfirmation));

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Failure<User>());

        // Act
        Result result = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        result.Failed.Should().BeTrue();
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithFailingUserAddition_DataTransactionHandlerStartDbTransactionCalled()
    {
        // Arrange
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Failure<User>());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _dataTransactionHandlerMock.Received(1).StartDbTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteRegistrationAsync_CallWithFailingUserAddition_HashGeneratorGenerateCalled()
    {
        // Arrange
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Failure<User>());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _hashGeneratorMock.Received(1).GenerateAsync(registrationRequest.Password, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteRegistrationAsync_CallWithFailingUserAddition_UserManagerAddCalled()
    {
        // Arrange
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Failure<User>());

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Failure<User>());

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Failure<User>());

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Failure<User>());

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Failure<User>());

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Failure<User>());

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Failure<User>());

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<RegistrationConfirmation>());

        // Act
        Result result = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        result.Failed.Should().BeTrue();
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithFailingRegistrationConfirmationAddition_DataTransactionHandlerStartDbTransactionCalled()
    {
        // Arrange
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<RegistrationConfirmation>());

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<RegistrationConfirmation>());

        // Act
        _ = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        await _hashGeneratorMock.Received(1).GenerateAsync(registrationRequest.Password, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteRegistrationAsync_CallWithFailingRegistrationConfirmationAddition_UserManagerAddCalled()
    {
        // Arrange
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<RegistrationConfirmation>());

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<RegistrationConfirmation>());

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<RegistrationConfirmation>());

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<RegistrationConfirmation>());

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<RegistrationConfirmation>());

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<RegistrationConfirmation>());

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<RegistrationConfirmation>());

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();
        RegistrationConfirmation newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Success(newlyCreatedRegistrationConfirmation));

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(newlyCreatedUser)),
                Arg.Any<CancellationToken>()).Returns(Result.Failure());

        // Act
        Result result = await _sut.ExecuteRegistrationAsync(registrationRequest);

        // Assert
        result.Failed.Should().BeTrue();
    }

    [Fact]
    public async Task
        ExecuteRegistrationAsync_CallWithFailingEmailSending_DataTransactionHandlerStartDbTransactionCalled()
    {
        // Arrange
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();
        RegistrationConfirmation newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Success(newlyCreatedRegistrationConfirmation));

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();
        RegistrationConfirmation newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Success(newlyCreatedRegistrationConfirmation));

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();
        RegistrationConfirmation newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Success(newlyCreatedRegistrationConfirmation));

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();
        RegistrationConfirmation newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Success(newlyCreatedRegistrationConfirmation));

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();
        RegistrationConfirmation newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Success(newlyCreatedRegistrationConfirmation));

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();
        RegistrationConfirmation newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Success(newlyCreatedRegistrationConfirmation));

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();
        RegistrationConfirmation newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Success(newlyCreatedRegistrationConfirmation));

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();
        RegistrationConfirmation newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Success(newlyCreatedRegistrationConfirmation));

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
        RegistrationWorkflowRequest registrationRequest = _fixture.Create<RegistrationWorkflowRequest>();
        User newlyCreatedUser = _fixture.Create<User>();
        RegistrationConfirmation newlyCreatedRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _userManagerMock
            .AddNewUserAsync(Arg.Is<User>(user => user.UserName.Equals(registrationRequest.Username)),
                Arg.Any<CancellationToken>()).Returns(Result.Success(newlyCreatedUser));

        _registrationConfirmationManagerMock
            .RegisterAsync(
                Arg.Is<RegistrationConfirmation>(registrationConfirmation =>
                    registrationConfirmation.User == newlyCreatedUser), Arg.Any<CancellationToken>())
            .Returns(Result.Success(newlyCreatedRegistrationConfirmation));

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
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();
        RegistrationConfirmation existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(existingRegistrationConfirmation));

        _registrationConfirmationManagerMock
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>()).Returns(Result.Success());

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(existingRegistrationConfirmation.User)),
                Arg.Any<CancellationToken>()).Returns(Result.Success());

        // Act
        Result result = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithExistingRegistrationConfirmationAndSuccessfulSubCalls_DataTransactionHandlerStartDbTransactionCalled()
    {
        // Arrange
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();
        RegistrationConfirmation existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(existingRegistrationConfirmation));

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
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();
        RegistrationConfirmation existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(existingRegistrationConfirmation));

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
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();
        RegistrationConfirmation existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(existingRegistrationConfirmation));

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
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();
        RegistrationConfirmation existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(existingRegistrationConfirmation));

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
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();
        RegistrationConfirmation existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(existingRegistrationConfirmation));

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
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();
        RegistrationConfirmation existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(existingRegistrationConfirmation));

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
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();
        RegistrationConfirmation existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(existingRegistrationConfirmation));

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
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<RegistrationConfirmation>());

        // Act
        Result result = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        result.Failed.Should().BeTrue();
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithFailingRegistrationConfirmationQueryCall_DataTransactionHandlerStartDbTransactionNotCalled()
    {
        // Arrange
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<RegistrationConfirmation>());

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
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<RegistrationConfirmation>());

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
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<RegistrationConfirmation>());

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
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<RegistrationConfirmation>());

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
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<RegistrationConfirmation>());

        // Act
        _ = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        await _dataTransactionHandlerMock.DidNotReceive().RollbackDbTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteConfirmationAsync_CallWithFailingConfirmation_Fails()
    {
        // Arrange
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();
        RegistrationConfirmation existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(existingRegistrationConfirmation));

        _registrationConfirmationManagerMock
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>()).Returns(Result.Failure());

        // Act
        Result result = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        result.Failed.Should().BeTrue();
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithFailingConfirmation_DataTransactionHandlerStartDbTransactionCalled()
    {
        // Arrange
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();
        RegistrationConfirmation existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(existingRegistrationConfirmation));

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
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();
        RegistrationConfirmation existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(existingRegistrationConfirmation));

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
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();
        RegistrationConfirmation existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(existingRegistrationConfirmation));

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
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();
        RegistrationConfirmation existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(existingRegistrationConfirmation));

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
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();
        RegistrationConfirmation existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(existingRegistrationConfirmation));

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
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();
        RegistrationConfirmation existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(existingRegistrationConfirmation));

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
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();
        RegistrationConfirmation existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(existingRegistrationConfirmation));

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
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();
        RegistrationConfirmation existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(existingRegistrationConfirmation));

        _registrationConfirmationManagerMock
            .ConfirmAsync(existingRegistrationConfirmation, Arg.Any<CancellationToken>()).Returns(Result.Success());

        _emailManagerMock
            .SendEmailAsync(Arg.Is<Email>(email => email.Receivers.Contains(existingRegistrationConfirmation.User)),
                Arg.Any<CancellationToken>()).Returns(Result.Failure());

        // Act
        Result result = await _sut.ExecuteConfirmationAsync(confirmationRequest);

        // Assert
        result.Failed.Should().BeTrue();
    }

    [Fact]
    public async Task
        ExecuteConfirmationAsync_CallWithFailingEmailSending_DataTransactionHandlerStartDbTransactionCalled()
    {
        // Arrange
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();
        RegistrationConfirmation existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(existingRegistrationConfirmation));

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
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();
        RegistrationConfirmation existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(existingRegistrationConfirmation));

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
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();
        RegistrationConfirmation existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(existingRegistrationConfirmation));

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
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();
        RegistrationConfirmation existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(existingRegistrationConfirmation));

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
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();
        RegistrationConfirmation existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(existingRegistrationConfirmation));

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
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();
        RegistrationConfirmation existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(existingRegistrationConfirmation));

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
        ConfirmationWorkflowRequest confirmationRequest = _fixture.Create<ConfirmationWorkflowRequest>();
        RegistrationConfirmation existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationManagerMock
            .GetRegistrationConfirmationWithUserAsync(confirmationRequest.ConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(existingRegistrationConfirmation));

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