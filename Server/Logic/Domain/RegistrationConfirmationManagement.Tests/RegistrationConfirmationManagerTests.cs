using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.InfrastructureTypes;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Data.Inclusion;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.RegistrationConfirmationManagement.Tests;

public class RegistrationConfirmationManagerTests
{
    private readonly Fixture _fixture;

    private readonly IRepository<Guid, RegistrationConfirmation> _registrationConfirmationRepositoryMock;

    private readonly RegistrationConfirmationManager _sut;

    public RegistrationConfirmationManagerTests()
    {
        _fixture = new Fixture();
        _fixture.Register(() => DateOnly.FromDateTime(_fixture.Create<DateTime>()));
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _registrationConfirmationRepositoryMock = Substitute.For<IRepository<Guid, RegistrationConfirmation>>();

        var repositoryFactoryMock = Substitute.For<IRepositoryFactory>();
        repositoryFactoryMock.CreateRepository<Guid, RegistrationConfirmation>()
            .Returns(_registrationConfirmationRepositoryMock);

        _sut = new RegistrationConfirmationManager(repositoryFactoryMock);
    }

    [Fact]
    public async Task RegisterAsync_CallWithoutExistingRegistrationConfirmations_Succeeds()
    {
        // Arrange
        var registrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationRepositoryMock.QueryAsync(Arg.Any<Inclusion[]>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<RegistrationConfirmation>().AsQueryable());

        // Act
        var result = await _sut.RegisterAsync(registrationConfirmation);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterAsync_CallWithoutExistingRegistrationConfirmations_RepositoryInsertCalled()
    {
        // Arrange
        var registrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationRepositoryMock.QueryAsync(Arg.Any<Inclusion[]>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<RegistrationConfirmation>().AsQueryable());

        // Act
        _ = await _sut.RegisterAsync(registrationConfirmation);

        // Assert
        await _registrationConfirmationRepositoryMock.Received(1)
            .InsertAsync(registrationConfirmation, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        RegisterAsync_CallWithoutExistingRegistrationConfirmations_ReturnsNewlyCreatedRegistrationConfirmation()
    {
        // Arrange
        var registrationConfirmation = _fixture.Create<RegistrationConfirmation>();
        var createdRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationRepositoryMock.QueryAsync(Arg.Any<Inclusion[]>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<RegistrationConfirmation>().AsQueryable());
        _registrationConfirmationRepositoryMock.InsertAsync(registrationConfirmation)
            .Returns(createdRegistrationConfirmation);

        // Act
        var result = await _sut.RegisterAsync(registrationConfirmation);

        // Assert
        result.Value.Should().BeSameAs(createdRegistrationConfirmation);
    }

    [Fact]
    public async Task RegisterAsync_CallWithOneExistingRegistrationConfirmationForUser_Succeeds()
    {
        // Arrange
        var user = _fixture.Create<User>();
        user.RegistrationConfirmation.User = user;

        _registrationConfirmationRepositoryMock.QueryAsync(Arg.Any<Inclusion[]>(), Arg.Any<CancellationToken>())
            .Returns(new[] { user.RegistrationConfirmation }.AsQueryable());

        // Act
        var result = await _sut.RegisterAsync(user.RegistrationConfirmation);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterAsync_CallWithOneExistingRegistrationConfirmationForUser_RepositoryInsertNotCalled()
    {
        // Arrange
        var user = _fixture.Create<User>();
        user.RegistrationConfirmation.User = user;

        _registrationConfirmationRepositoryMock.QueryAsync(Arg.Any<Inclusion[]>(), Arg.Any<CancellationToken>())
            .Returns(new[] { user.RegistrationConfirmation }.AsQueryable());

        // Act
        _ = await _sut.RegisterAsync(user.RegistrationConfirmation);

        // Assert
        await _registrationConfirmationRepositoryMock.DidNotReceive()
            .InsertAsync(Arg.Any<RegistrationConfirmation>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        RegisterAsync_CallWithOneExistingRegistrationConfirmationForUser_ReturnsExistingRegistrationConfirmation()
    {
        // Arrange
        var user = _fixture.Create<User>();
        user.RegistrationConfirmation = null!;

        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.User, user));
        var existingRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();
        var newRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationRepositoryMock.QueryAsync(Arg.Any<Inclusion[]>(), Arg.Any<CancellationToken>())
            .Returns(new[] { existingRegistrationConfirmation }.AsQueryable());

        // Act
        var result = await _sut.RegisterAsync(newRegistrationConfirmation);

        // Assert
        result.Value.Should().BeSameAs(existingRegistrationConfirmation);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(25)]
    public async Task RegisterAsync_CallWithMultipleExistingRegistrationConfirmationsForUser_Fails(int existingRegistrationConfirmationsCount)
    {
        // Arrange
        var user = _fixture.Create<User>();
        user.RegistrationConfirmation.User = user;

        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.User, user));
        var existingRegistrationConfirmations = _fixture.CreateMany<RegistrationConfirmation>(existingRegistrationConfirmationsCount).AsQueryable();
        var newRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationRepositoryMock.QueryAsync(Arg.Any<Inclusion[]>(), Arg.Any<CancellationToken>())
            .Returns(existingRegistrationConfirmations);

        // Act
        var result = await _sut.RegisterAsync(newRegistrationConfirmation);

        // Assert
        result.Failed.Should().BeTrue();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(6)]
    [InlineData(13)]
    [InlineData(23)]
    public async Task RegisterAsync_CallWithMultipleExistingRegistrationConfirmationsForUser_ErrorCountIsOne(int existingRegistrationConfirmationsCount)
    {
        // Arrange
        var user = _fixture.Create<User>();
        user.RegistrationConfirmation.User = user;

        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.User, user));
        var existingRegistrationConfirmations = _fixture.CreateMany<RegistrationConfirmation>(existingRegistrationConfirmationsCount).AsQueryable();
        var newRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationRepositoryMock.QueryAsync(Arg.Any<Inclusion[]>(), Arg.Any<CancellationToken>())
            .Returns(existingRegistrationConfirmations);

        // Act
        var result = await _sut.RegisterAsync(newRegistrationConfirmation);

        // Assert
        result.Errors.Should().ContainSingle();
    }

    [Fact]
    public async Task ConfirmAsyncByRegistrationConfirmation_CallWithUnconfirmedRegistrationConfirmation_Succeeds()
    {
        // Arrange
        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.ConfirmationDate, null as DateTime?));
        var registrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        // Act
        var result = await _sut.ConfirmAsync(registrationConfirmation);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmAsyncByRegistrationConfirmation_CallWithUnconfirmedRegistrationConfirmation_RegistrationConfirmationIsConfirmed()
    {
        // Arrange
        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.ConfirmationDate, null as DateTime?));
        var registrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        // Act
        _ = await _sut.ConfirmAsync(registrationConfirmation);

        // Assert
        registrationConfirmation.RegistrationIsConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmAsyncByRegistrationConfirmation_CallWithUnconfirmedRegistrationConfirmation_RepositoryUpdateCalled()
    {
        // Arrange
        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.ConfirmationDate, null as DateTime?));
        var registrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        // Act
        _ = await _sut.ConfirmAsync(registrationConfirmation);

        // Assert
        await _registrationConfirmationRepositoryMock.Received(1)
            .UpdateAsync(registrationConfirmation, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfirmAsyncByRegistrationConfirmation_CallWithConfirmedRegistrationConfirmation_Succeeds()
    {
        // Arrange
        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.ConfirmationDate, _fixture.Create<DateTime>()));
        var registrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        // Act
        var result = await _sut.ConfirmAsync(registrationConfirmation);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmAsyncByRegistrationConfirmation_CallWithConfirmedRegistrationConfirmation_RegistrationConfirmationIsConfirmed()
    {
        // Arrange
        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.ConfirmationDate, _fixture.Create<DateTime>()));
        var registrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        // Act
        _ = await _sut.ConfirmAsync(registrationConfirmation);

        // Assert
        registrationConfirmation.RegistrationIsConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmAsyncByRegistrationConfirmation_CallWithConfirmedRegistrationConfirmation_RepositoryUpdateNotCalled()
    {
        // Arrange
        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.ConfirmationDate, _fixture.Create<DateTime>()));
        var registrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        // Act
        _ = await _sut.ConfirmAsync(registrationConfirmation);

        // Assert
        await _registrationConfirmationRepositoryMock.DidNotReceive()
            .UpdateAsync(Arg.Any<RegistrationConfirmation>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfirmAsyncById_CallWithUnconfirmedRegistrationConfirmation_Succeeds()
    {
        // Arrange
        var registrationConfirmationId = _fixture.Create<Guid>();
        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.ConfirmationDate, null as DateTime?));
        var registrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationRepositoryMock.FindAsync(registrationConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(registrationConfirmation));

        // Act
        var result = await _sut.ConfirmAsync(registrationConfirmationId);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmAsyncById_CallWithUnconfirmedRegistrationConfirmation_RegistrationConfirmationIsConfirmed()
    {
        // Arrange
        var registrationConfirmationId = _fixture.Create<Guid>();
        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.ConfirmationDate, null as DateTime?));
        var registrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationRepositoryMock.FindAsync(registrationConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(registrationConfirmation));

        // Act
        _ = await _sut.ConfirmAsync(registrationConfirmationId);

        // Assert
        registrationConfirmation.RegistrationIsConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmAsyncById_CallWithUnconfirmedRegistrationConfirmation_RepositoryUpdateCalled()
    {
        // Arrange
        var registrationConfirmationId = _fixture.Create<Guid>();
        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.ConfirmationDate, null as DateTime?));
        var registrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationRepositoryMock.FindAsync(registrationConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(registrationConfirmation));

        // Act
        _ = await _sut.ConfirmAsync(registrationConfirmationId);

        // Assert
        await _registrationConfirmationRepositoryMock.Received(1)
            .UpdateAsync(registrationConfirmation, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfirmAsyncById_CallWithConfirmedRegistrationConfirmation_Succeeds()
    {
        // Arrange
        var registrationConfirmationId = _fixture.Create<Guid>();
        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.ConfirmationDate, _fixture.Create<DateTime>()));
        var registrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationRepositoryMock.FindAsync(registrationConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(registrationConfirmation));

        // Act
        var result = await _sut.ConfirmAsync(registrationConfirmationId);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmAsyncById_CallWithConfirmedRegistrationConfirmation_RegistrationConfirmationIsConfirmed()
    {
        // Arrange
        var registrationConfirmationId = _fixture.Create<Guid>();
        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.ConfirmationDate, _fixture.Create<DateTime>()));
        var registrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationRepositoryMock.FindAsync(registrationConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(registrationConfirmation));

        // Act
        _ = await _sut.ConfirmAsync(registrationConfirmationId);

        // Assert
        registrationConfirmation.RegistrationIsConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmAsyncById_CallWithConfirmedRegistrationConfirmation_RepositoryUpdateNotCalled()
    {
        // Arrange
        var registrationConfirmationId = _fixture.Create<Guid>();
        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.ConfirmationDate, _fixture.Create<DateTime>()));
        var registrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationRepositoryMock.FindAsync(registrationConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Success(registrationConfirmation));

        // Act
        _ = await _sut.ConfirmAsync(registrationConfirmationId);

        // Assert
        await _registrationConfirmationRepositoryMock.DidNotReceive()
            .UpdateAsync(Arg.Any<RegistrationConfirmation>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfirmAsyncById_CallWithUnknownId_Fails()
    {
        // Arrange
        var registrationConfirmationId = _fixture.Create<Guid>();

        _registrationConfirmationRepositoryMock.FindAsync(registrationConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Failure());

        // Act
        var result = await _sut.ConfirmAsync(registrationConfirmationId);

        // Assert
        result.Failed.Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmAsyncById_CallWithUnknownId_ErrorsGetHandedThroughFromRepository()
    {
        // Arrange
        var registrationConfirmationId = _fixture.Create<Guid>();
        var errors = _fixture.CreateMany<Error>().ToArray();

        _registrationConfirmationRepositoryMock.FindAsync(registrationConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Failure(errors));

        // Act
        var result = await _sut.ConfirmAsync(registrationConfirmationId);

        // Assert
        result.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public async Task ConfirmAsyncById_CallWithUnknownId_RepositoryUpdateNotCalled()
    {
        // Arrange
        var registrationConfirmationId = _fixture.Create<Guid>();
        var errors = _fixture.CreateMany<Error>().ToArray();

        _registrationConfirmationRepositoryMock.FindAsync(registrationConfirmationId, Arg.Any<CancellationToken>())
            .Returns(Result<RegistrationConfirmation>.Failure(errors));

        // Act
        _ = await _sut.ConfirmAsync(registrationConfirmationId);

        // Assert
        await _registrationConfirmationRepositoryMock.DidNotReceive()
            .UpdateAsync(Arg.Any<RegistrationConfirmation>(), Arg.Any<CancellationToken>());
    }
}