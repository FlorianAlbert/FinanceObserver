using System.Linq.Expressions;
using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.InfrastructureTypes;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Data;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.UserManagement.Tests;

public class UserManagerTests
{
    private readonly Fixture _fixture;

    private readonly IRepository<Guid, User> _repositoryMock;

    private readonly UserManager _sut;

    public UserManagerTests()
    {
        _fixture = new Fixture();
        _fixture.Register(() => DateOnly.FromDateTime(_fixture.Create<DateTime>()));
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _repositoryMock = Substitute.For<IRepository<Guid, User>>();

        var repositoryFactoryMock = Substitute.For<IRepositoryFactory>();
        repositoryFactoryMock.CreateRepository<Guid, User>().Returns(_repositoryMock);

        _sut = new UserManager(repositoryFactoryMock);
    }

    [Fact]
    public async Task AddNewUserAsync_CallWithoutExistingUser_Succeeds()
    {
        // Arrange
        var user = _fixture.Create<User>();

        _repositoryMock.ExistsAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<Inclusion<Guid, User>[]>(),
            Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var result = await _sut.AddNewUserAsync(user);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task AddNewUserAsync_CallWithoutExistingUser_RepositoryInsertCalled()
    {
        // Arrange
        var user = _fixture.Create<User>();

        _repositoryMock.ExistsAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<Inclusion<Guid, User>[]>(),
            Arg.Any<CancellationToken>()).Returns(false);

        // Act
        _ = await _sut.AddNewUserAsync(user);

        // Assert
        await _repositoryMock.Received(1).InsertAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddNewUserAsync_CallWithoutExistingUser_ReturnsNewUserReturnedByRepository()
    {
        // Arrange
        var user = _fixture.Create<User>();
        var createdUser = _fixture.Create<User>();

        _repositoryMock.ExistsAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<Inclusion<Guid, User>[]>(),
            Arg.Any<CancellationToken>()).Returns(false);
        _repositoryMock.InsertAsync(user, Arg.Any<CancellationToken>()).Returns(createdUser);

        // Act
        var result = await _sut.AddNewUserAsync(user);

        // Assert
        result.Value.Should().BeSameAs(createdUser);
    }

    [Fact]
    public async Task AddNewUserAsync_CallWithExistingUser_Fails()
    {
        // Arrange
        var user = _fixture.Create<User>();

        _repositoryMock.ExistsAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<Inclusion<Guid, User>[]>(),
            Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _sut.AddNewUserAsync(user);

        // Assert
        result.Failed.Should().BeTrue();
    }

    [Fact]
    public async Task AddNewUserAsync_CallWithExistingUser_RepositoryInsertNotCalled()
    {
        // Arrange
        var user = _fixture.Create<User>();

        _repositoryMock.ExistsAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<Inclusion<Guid, User>[]>(),
            Arg.Any<CancellationToken>()).Returns(true);

        // Act
        _ = await _sut.AddNewUserAsync(user);

        // Assert
        await _repositoryMock.DidNotReceive().InsertAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddNewUserAsync_CallWithExistingUser_ReturnErrorCountIsOne()
    {
        // Arrange
        var user = _fixture.Create<User>();

        _repositoryMock.ExistsAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<Inclusion<Guid, User>[]>(),
            Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _sut.AddNewUserAsync(user);

        // Assert
        result.Errors.Should().ContainSingle();
    }

    [Fact]
    public async Task RemoveUserAsync_Call_Succeeds()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();

        // Act
        var result = await _sut.RemoveUserAsync(userId);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveUserAsync_Call_RepositoryDeleteCalled()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();

        // Act
        _ = await _sut.RemoveUserAsync(userId);

        // Assert
        await _repositoryMock.Received(1).DeleteAsync(userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveUsersAsync_Call_Succeeds()
    {
        // Arrange
        var users = _fixture.CreateMany<User>().AsQueryable();

        // Act
        var result = await _sut.RemoveUsersAsync(users);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveUsersAsync_Call_RepositoryDeleteCalled()
    {
        // Arrange
        var users = _fixture.CreateMany<User>().AsQueryable();

        // Act
        _ = await _sut.RemoveUsersAsync(users);

        // Assert
        await _repositoryMock.Received(1).DeleteAsync(users, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetUserAsync_CallWithSuccessfulRepositoryCall_Succeeds()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var user = _fixture.Create<User>();
        
        _repositoryMock.FindAsync(userId, Arg.Any<CancellationToken>()).Returns(Result<User>.Success(user));

        // Act
        var result = await _sut.GetUserAsync(userId);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task GetUserAsync_CallWithSuccessfulRepositoryCall_ReturnsUserReturnedByRepository()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var user = _fixture.Create<User>();
        
        _repositoryMock.FindAsync(userId, Arg.Any<CancellationToken>()).Returns(Result<User>.Success(user));

        // Act
        var result = await _sut.GetUserAsync(userId);

        // Assert
        result.Value.Should().BeSameAs(user);
    }

    [Fact]
    public async Task GetUserAsync_CallWithFailedRepositoryCall_Fails()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        
        _repositoryMock.FindAsync(userId, Arg.Any<CancellationToken>()).Returns(Result<User>.Failure());

        // Act
        var result = await _sut.GetUserAsync(userId);

        // Assert
        result.Failed.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(25)]
    public async Task GetUserAsync_CallWithFailedRepositoryCall_ReturnedErrorsMatchErrorsReturnedFromRepository(int repositoryErrorCount)
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var errors = _fixture.CreateMany<Error>(repositoryErrorCount).ToArray();
        
        _repositoryMock.FindAsync(userId, Arg.Any<CancellationToken>()).Returns(Result<User>.Failure(errors));

        // Act
        var result = await _sut.GetUserAsync(userId);

        // Assert
        result.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public async Task GetAllUsersAsync_Call_Succeeds()
    {
        // Arrange

        // Act
        var result = await _sut.GetAllUsersAsync();

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllUsersAsync_Call_RepositoryQueryCalled()
    {
        // Arrange

        // Act
        _ = await _sut.GetAllUsersAsync();

        // Assert
        await _repositoryMock.Received(1).QueryAsync(Arg.Any<Inclusion<Guid, User>[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllUsersAsync_Call_ReturnsUsersReturnedFromRepository()
    {
        // Arrange
        var users = _fixture.CreateMany<User>().AsQueryable();

        _repositoryMock.QueryAsync(Arg.Any<Inclusion<Guid, User>[]>(), Arg.Any<CancellationToken>()).Returns(users);

        // Act
        var result = await _sut.GetAllUsersAsync();

        // Assert
        result.Value.Should().BeSameAs(users);
    }
}