using FlorianAlbert.FinanceObserver.Server.Logic.Domain.DataTransactionHandling.Contract;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.EmailManagement.Contract;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.HashHandling.Contract;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.RegistrationConfirmationManagement.Contract;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.UserManagement.Contract;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Business.RegistrationWorkflow.Tests;

public class RegistrationWorkflowTests
{
    private readonly IDataTransactionHandler _dataTransactionHandlerMock;
    private readonly IUserManager _userManagerMock;
    private readonly IHashGenerator _hashGeneratorMock;
    private readonly IEmailManager _emailManagerMock;
    private readonly IRegistrationConfirmationManager _registrationConfirmationManagerMock;

    private readonly RegistrationWorkflow _sut;

    public RegistrationWorkflowTests()
    {
        _dataTransactionHandlerMock = Substitute.For<IDataTransactionHandler>();
        _userManagerMock = Substitute.For<IUserManager>();
        _hashGeneratorMock = Substitute.For<IHashGenerator>();
        _emailManagerMock = Substitute.For<IEmailManager>();
        _registrationConfirmationManagerMock = Substitute.For<IRegistrationConfirmationManager>();

        _sut = new RegistrationWorkflow(_dataTransactionHandlerMock, _userManagerMock, _hashGeneratorMock,
            _emailManagerMock, _registrationConfirmationManagerMock);
    }

    [Fact]
    public void Test1()
    {
    }
}