using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.InfrastructureTypes;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.RegistrationConfirmationManagement.Contract;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.UserManagement.Contract;
using Microsoft.Extensions.DependencyInjection;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Business.RegistrationWorkflow.Tests;

public class ExpiredRegistrationsUserDeletionServiceTests
{
    private readonly Fixture _fixture;

    private readonly IRegistrationConfirmationManager _registrationConfirmationManagerMock;
    private readonly IUserManager _userManagerMock;

    private readonly ExpiredRegistrationsUserDeletionService _sut;

    public ExpiredRegistrationsUserDeletionServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Register(() => DateOnly.FromDateTime(_fixture.Create<DateTime>()));
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _registrationConfirmationManagerMock = Substitute.For<IRegistrationConfirmationManager>();
        _userManagerMock = Substitute.For<IUserManager>();

        var scopeServiceProviderMock = Substitute.For<IServiceProvider>();
        scopeServiceProviderMock.GetService(typeof(IRegistrationConfirmationManager))
            .Returns(_registrationConfirmationManagerMock);
        scopeServiceProviderMock.GetService(typeof(IUserManager)).Returns(_userManagerMock);

        var serviceScopeMock = Substitute.For<IServiceScope>();
        serviceScopeMock.ServiceProvider.Returns(scopeServiceProviderMock);

        var serviceScopeFactoryMock = Substitute.For<IServiceScopeFactory>();
        serviceScopeFactoryMock.CreateScope().Returns(serviceScopeMock);

        var globalServiceProviderMock = Substitute.For<IServiceProvider>();
        globalServiceProviderMock.GetService(typeof(IServiceScopeFactory)).Returns(serviceScopeFactoryMock);

        _sut = new ExpiredRegistrationsUserDeletionService(globalServiceProviderMock, 1);
    }

    [Fact]
    public async Task StartAsync_CallWithSuccessfulUnconfirmedRegistrationConfirmationsQuery_UserManagerRemoveCalled()
    {
        // Arrange
        var timeOut = TimeSpan.FromMinutes(3);

        var cancellationTokenSource = new CancellationTokenSource(timeOut);
        var taskCompletionSource = new TaskCompletionSource();
        cancellationTokenSource.Token.Register(() => taskCompletionSource.TrySetCanceled(),
            useSynchronizationContext: false);

        _registrationConfirmationManagerMock
            .GetUnconfirmedRegistrationConfirmationsWithUserAsync(Arg.Any<CancellationToken>()).Returns(
                Result<IQueryable<RegistrationConfirmation>>.Success(Array.Empty<RegistrationConfirmation>()
                    .AsQueryable()));

        _userManagerMock
            .When(u => u.RemoveUsersAsync(Arg.Any<IEnumerable<User>>(), Arg.Any<CancellationToken>()))
            .Do(_ => taskCompletionSource.TrySetResult());

        // Act
        await _sut.StartAsync(cancellationTokenSource.Token);
        await taskCompletionSource.Task;
        await cancellationTokenSource.CancelAsync();

        // Assert
        var callCount =
            _userManagerMock.ReceivedCalls().Count(call =>
                call.GetMethodInfo().Equals(typeof(IUserManager).GetMethod(
                    nameof(IUserManager.RemoveUsersAsync))));
        callCount.Should().Be(1);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(2, 0)]
    [InlineData(5, 0)]
    [InlineData(10, 0)]
    [InlineData(25, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(5, 1)]
    [InlineData(10, 1)]
    [InlineData(25, 1)]
    [InlineData(2, 2)]
    [InlineData(5, 2)]
    [InlineData(10, 2)]
    [InlineData(25, 2)]
    [InlineData(5, 3)]
    [InlineData(10, 3)]
    [InlineData(25, 3)]
    [InlineData(5, 5)]
    [InlineData(10, 5)]
    [InlineData(25, 5)]
    [InlineData(10, 8)]
    [InlineData(25, 8)]
    [InlineData(10, 10)]
    [InlineData(25, 10)]
    [InlineData(25, 21)]
    [InlineData(25, 25)]
    public async Task
        StartAsync_CallWithSuccessfulUnconfirmedRegistrationConfirmationsQuery_UserManagerRemoveCalledWithCorrectUsers(
            int unconfirmedRegistrationConfirmationsCount, int expiredUnconfirmedRegistrationConfirmationsCount)
    {
        // Arrange
        var expiredCreatedDate = DateTime.UtcNow - TimeSpan.FromDays(5);
        var expiredRegistrationConfirmations =
            _fixture.CreateMany<RegistrationConfirmation>(expiredUnconfirmedRegistrationConfirmationsCount).ToList();
        expiredRegistrationConfirmations.ForEach(r => r.CreatedDate = expiredCreatedDate);
        var usersToDelete = expiredRegistrationConfirmations.Select(r => r.User).ToList();

        var nonExpiredCreatedDate = DateTime.UtcNow - TimeSpan.FromMinutes(5);
        var nonExpiredRegistrationConfirmations =
            _fixture.CreateMany<RegistrationConfirmation>(unconfirmedRegistrationConfirmationsCount -
                                                          expiredUnconfirmedRegistrationConfirmationsCount).ToList();
        nonExpiredRegistrationConfirmations.ForEach(r => r.CreatedDate = nonExpiredCreatedDate);

        var timeOut = TimeSpan.FromMinutes(3);

        var cancellationTokenSource = new CancellationTokenSource(timeOut);
        var taskCompletionSource = new TaskCompletionSource();
        cancellationTokenSource.Token.Register(() => taskCompletionSource.TrySetCanceled(),
            useSynchronizationContext: false);
        
        // Combine and shuffle expired and non-expired RegistrationConfirmations
        var random = new Random();
        var returnedRegistrationConfirmations =
            expiredRegistrationConfirmations
                .Concat(nonExpiredRegistrationConfirmations)
                .Select(existingRegistration => new { orderKey = random.Next(), existingRegistration })
                .OrderBy(tmp => tmp.orderKey)
                .Select(x => x.existingRegistration)
                .AsQueryable();

        _registrationConfirmationManagerMock
            .GetUnconfirmedRegistrationConfirmationsWithUserAsync(Arg.Any<CancellationToken>()).Returns(
                Result<IQueryable<RegistrationConfirmation>>.Success(returnedRegistrationConfirmations));

        _userManagerMock
            .When(u => u.RemoveUsersAsync(
                Arg.Is<IEnumerable<User>>(users =>
                    // ReSharper disable PossibleMultipleEnumeration
                    users.Count() == usersToDelete.Count && usersToDelete.TrueForAll(users.Contains)),
                    // ReSharper restore PossibleMultipleEnumeration
                Arg.Any<CancellationToken>()))
            .Do(_ => taskCompletionSource.TrySetResult());

        // Act
        await _sut.StartAsync(cancellationTokenSource.Token);
        await taskCompletionSource.Task;
        await cancellationTokenSource.CancelAsync();

        // Assert
        var callCount =
            _userManagerMock.ReceivedCalls().Count(call =>
                call.GetMethodInfo().Equals(typeof(IUserManager).GetMethod(
                    nameof(IUserManager.RemoveUsersAsync))));
        callCount.Should().Be(1);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(25)]
    public async Task StartAsync_CallWithSuccessfulUnconfirmedRegistrationConfirmationsQuery_UserManagerRemoveCalledNTimesAfterCancellation(int requiredCallNumber)
    {
        // Arrange
        var timeOut = TimeSpan.FromMinutes(3);

        var cancellationTokenSource = new CancellationTokenSource(timeOut);
        var taskCompletionSource = new TaskCompletionSource();
        cancellationTokenSource.Token.Register(() => taskCompletionSource.TrySetCanceled(),
            useSynchronizationContext: false);

        _registrationConfirmationManagerMock
            .GetUnconfirmedRegistrationConfirmationsWithUserAsync(Arg.Any<CancellationToken>()).Returns(
                Result<IQueryable<RegistrationConfirmation>>.Success(Array.Empty<RegistrationConfirmation>()
                    .AsQueryable()));

        var counter = 0;
        _userManagerMock
            .When(u => u.RemoveUsersAsync(Arg.Any<IEnumerable<User>>(), Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                counter++;

                if (counter == requiredCallNumber)
                {
                    taskCompletionSource.TrySetResult();
                }
            });

        // Act
        await _sut.StartAsync(cancellationTokenSource.Token);
        await taskCompletionSource.Task;
        await cancellationTokenSource.CancelAsync();

        // Assert
        var callCount =
            _userManagerMock.ReceivedCalls().Count(call =>
                call.GetMethodInfo().Equals(typeof(IUserManager).GetMethod(
                    nameof(IUserManager.RemoveUsersAsync))));
        callCount.Should().Be(requiredCallNumber);
    }

    [Fact]
    public async Task StartAsync_CallWithFailingUnconfirmedRegistrationConfirmationsQuery_UserManagerRemoveNotCalled()
    {
        // Arrange
        var timeOut = TimeSpan.FromMinutes(3);

        var cancellationTokenSource = new CancellationTokenSource(timeOut);
        var taskCompletionSource = new TaskCompletionSource();
        cancellationTokenSource.Token.Register(() => taskCompletionSource.TrySetCanceled(),
            useSynchronizationContext: false);

        var queryCount = 0;
        _registrationConfirmationManagerMock.When(r => r.GetUnconfirmedRegistrationConfirmationsWithUserAsync(Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                queryCount++;

                if (queryCount > 1)
                {
                    taskCompletionSource.TrySetResult();
                }
            });

        _registrationConfirmationManagerMock
            .GetUnconfirmedRegistrationConfirmationsWithUserAsync(Arg.Any<CancellationToken>()).Returns(
                Result<IQueryable<RegistrationConfirmation>>.Failure());

        // Act
        await _sut.StartAsync(cancellationTokenSource.Token);
        await taskCompletionSource.Task;
        await cancellationTokenSource.CancelAsync();

        // Assert
        var callCount =
            _userManagerMock.ReceivedCalls().Count(call =>
                call.GetMethodInfo().Equals(typeof(IUserManager).GetMethod(
                    nameof(IUserManager.RemoveUsersAsync))));
        callCount.Should().Be(0);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(25)]
    public async Task StartAsync_CallWithFailingUnconfirmedRegistrationConfirmationsQuery_UserManagerRemoveNotCalledAfterCancellation(int requiredCallNumber)
    {
        // Arrange
        var timeOut = TimeSpan.FromMinutes(3);

        var cancellationTokenSource = new CancellationTokenSource(timeOut);
        var taskCompletionSource = new TaskCompletionSource();
        cancellationTokenSource.Token.Register(() => taskCompletionSource.TrySetCanceled(),
            useSynchronizationContext: false);

        _registrationConfirmationManagerMock
            .GetUnconfirmedRegistrationConfirmationsWithUserAsync(Arg.Any<CancellationToken>()).Returns(
                Result<IQueryable<RegistrationConfirmation>>.Failure());

        var queryCount = 0;
        _registrationConfirmationManagerMock.When(r => r.GetUnconfirmedRegistrationConfirmationsWithUserAsync(Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                queryCount++;

                if (queryCount > requiredCallNumber)
                {
                    taskCompletionSource.TrySetResult();
                }
            });

        // Act
        await _sut.StartAsync(cancellationTokenSource.Token);
        await taskCompletionSource.Task;
        await cancellationTokenSource.CancelAsync();

        // Assert
        var callCount =
            _userManagerMock.ReceivedCalls().Count(call =>
                call.GetMethodInfo().Equals(typeof(IUserManager).GetMethod(
                    nameof(IUserManager.RemoveUsersAsync))));
        callCount.Should().Be(0);
    }
}