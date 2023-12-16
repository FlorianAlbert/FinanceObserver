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
}