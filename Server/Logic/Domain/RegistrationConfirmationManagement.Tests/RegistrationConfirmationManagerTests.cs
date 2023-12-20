using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.InfrastructureTypes;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Data;
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

    // RegisterAsync Tests

    [Fact]
    public async Task RegisterAsync_CallWithoutExistingRegistrationConfirmations_Succeeds()
    {
        // Arrange
        var registrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationRepositoryMock.QueryAsync(Arg.Any<Inclusion<Guid, RegistrationConfirmation>[]>(), Arg.Any<CancellationToken>())
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

        _registrationConfirmationRepositoryMock.QueryAsync(Arg.Any<Inclusion<Guid, RegistrationConfirmation>[]>(), Arg.Any<CancellationToken>())
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

        _registrationConfirmationRepositoryMock.QueryAsync(Arg.Any<Inclusion<Guid, RegistrationConfirmation>[]>(), Arg.Any<CancellationToken>())
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

        _registrationConfirmationRepositoryMock.QueryAsync(Arg.Any<Inclusion<Guid, RegistrationConfirmation>[]>(), Arg.Any<CancellationToken>())
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

        _registrationConfirmationRepositoryMock.QueryAsync(Arg.Any<Inclusion<Guid, RegistrationConfirmation>[]>(), Arg.Any<CancellationToken>())
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

        _registrationConfirmationRepositoryMock.QueryAsync(Arg.Any<Inclusion<Guid, RegistrationConfirmation>[]>(), Arg.Any<CancellationToken>())
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
    public async Task RegisterAsync_CallWithMultipleExistingRegistrationConfirmationsForUser_Fails(
        int existingRegistrationConfirmationsCount)
    {
        // Arrange
        var user = _fixture.Create<User>();
        user.RegistrationConfirmation.User = user;

        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.User, user));
        var existingRegistrationConfirmations = _fixture
            .CreateMany<RegistrationConfirmation>(existingRegistrationConfirmationsCount).AsQueryable();
        var newRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationRepositoryMock.QueryAsync(Arg.Any<Inclusion<Guid, RegistrationConfirmation>[]>(), Arg.Any<CancellationToken>())
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
    public async Task RegisterAsync_CallWithMultipleExistingRegistrationConfirmationsForUser_ErrorCountIsOne(
        int existingRegistrationConfirmationsCount)
    {
        // Arrange
        var user = _fixture.Create<User>();
        user.RegistrationConfirmation.User = user;

        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.User, user));
        var existingRegistrationConfirmations = _fixture
            .CreateMany<RegistrationConfirmation>(existingRegistrationConfirmationsCount).AsQueryable();
        var newRegistrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationRepositoryMock.QueryAsync(Arg.Any<Inclusion<Guid, RegistrationConfirmation>[]>(), Arg.Any<CancellationToken>())
            .Returns(existingRegistrationConfirmations);

        // Act
        var result = await _sut.RegisterAsync(newRegistrationConfirmation);

        // Assert
        result.Errors.Should().ContainSingle();
    }

    // ConfirmAsync(RegistrationConfirmation) tests

    [Fact]
    public async Task ConfirmAsyncByRegistrationConfirmation_CallWithUnconfirmedRegistrationConfirmation_Succeeds()
    {
        // Arrange
        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.ConfirmationDate,
                null as DateTime?));
        var registrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        // Act
        var result = await _sut.ConfirmAsync(registrationConfirmation);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task
        ConfirmAsyncByRegistrationConfirmation_CallWithUnconfirmedRegistrationConfirmation_RegistrationConfirmationIsConfirmed()
    {
        // Arrange
        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.ConfirmationDate,
                null as DateTime?));
        var registrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        // Act
        _ = await _sut.ConfirmAsync(registrationConfirmation);

        // Assert
        registrationConfirmation.RegistrationIsConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task
        ConfirmAsyncByRegistrationConfirmation_CallWithUnconfirmedRegistrationConfirmation_RepositoryUpdateCalled()
    {
        // Arrange
        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.ConfirmationDate,
                null as DateTime?));
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
            composerTransformation.With(registrationConfirmation => registrationConfirmation.ConfirmationDate,
                _fixture.Create<DateTime>()));
        var registrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        // Act
        var result = await _sut.ConfirmAsync(registrationConfirmation);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task
        ConfirmAsyncByRegistrationConfirmation_CallWithConfirmedRegistrationConfirmation_RegistrationConfirmationIsConfirmed()
    {
        // Arrange
        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.ConfirmationDate,
                _fixture.Create<DateTime>()));
        var registrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        // Act
        _ = await _sut.ConfirmAsync(registrationConfirmation);

        // Assert
        registrationConfirmation.RegistrationIsConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task
        ConfirmAsyncByRegistrationConfirmation_CallWithConfirmedRegistrationConfirmation_RepositoryUpdateNotCalled()
    {
        // Arrange
        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.ConfirmationDate,
                _fixture.Create<DateTime>()));
        var registrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        // Act
        _ = await _sut.ConfirmAsync(registrationConfirmation);

        // Assert
        await _registrationConfirmationRepositoryMock.DidNotReceive()
            .UpdateAsync(Arg.Any<RegistrationConfirmation>(), Arg.Any<CancellationToken>());
    }

    // ConfirmAsync(Guid) tests

    [Fact]
    public async Task ConfirmAsyncById_CallWithUnconfirmedRegistrationConfirmation_Succeeds()
    {
        // Arrange
        var registrationConfirmationId = _fixture.Create<Guid>();
        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.ConfirmationDate,
                null as DateTime?));
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
            composerTransformation.With(registrationConfirmation => registrationConfirmation.ConfirmationDate,
                null as DateTime?));
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
            composerTransformation.With(registrationConfirmation => registrationConfirmation.ConfirmationDate,
                null as DateTime?));
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
            composerTransformation.With(registrationConfirmation => registrationConfirmation.ConfirmationDate,
                _fixture.Create<DateTime>()));
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
            composerTransformation.With(registrationConfirmation => registrationConfirmation.ConfirmationDate,
                _fixture.Create<DateTime>()));
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
            composerTransformation.With(registrationConfirmation => registrationConfirmation.ConfirmationDate,
                _fixture.Create<DateTime>()));
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

    // GetRegistrationConfirmationWithUserAsync tests

    [Fact]
    public async Task GetRegistrationConfirmationWithUserAsync_CallWithOneExistingRegistrationConfirmation_Succeeds()
    {
        // Arrange
        var registrationConfirmationId = _fixture.Create<Guid>();

        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.Id,
                registrationConfirmationId));
        var registrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationRepositoryMock.QueryAsync(Arg.Any<Inclusion<Guid, RegistrationConfirmation>[]>(), Arg.Any<CancellationToken>())
            .Returns(new[] { registrationConfirmation }.AsQueryable());

        // Act
        var result = await _sut.GetRegistrationConfirmationWithUserAsync(registrationConfirmationId);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task
        GetRegistrationConfirmationWithUserAsync_CallWithOneExistingRegistrationConfirmation_ReturnsExistingRegistrationConfirmation()
    {
        // Arrange
        var registrationConfirmationId = _fixture.Create<Guid>();

        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.Id,
                registrationConfirmationId));
        var registrationConfirmation = _fixture.Create<RegistrationConfirmation>();

        _registrationConfirmationRepositoryMock.QueryAsync(Arg.Any<Inclusion<Guid, RegistrationConfirmation>[]>(), Arg.Any<CancellationToken>())
            .Returns(new[] { registrationConfirmation }.AsQueryable());

        // Act
        var result = await _sut.GetRegistrationConfirmationWithUserAsync(registrationConfirmationId);

        // Assert
        result.Value.Should().BeSameAs(registrationConfirmation);
    }

    [Fact]
    public async Task GetRegistrationConfirmationWithUserAsync_CallWithoutExistingRegistrationConfirmation_Fails()
    {
        // Arrange
        var registrationConfirmationId = _fixture.Create<Guid>();
        _registrationConfirmationRepositoryMock.QueryAsync(Arg.Any<Inclusion<Guid, RegistrationConfirmation>[]>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<RegistrationConfirmation>().AsQueryable());

        // Act
        var result = await _sut.GetRegistrationConfirmationWithUserAsync(registrationConfirmationId);

        // Assert
        result.Failed.Should().BeTrue();
    }

    [Fact]
    public async Task
        GetRegistrationConfirmationWithUserAsync_CallWithoutExistingRegistrationConfirmation_ErrorCountIsOne()
    {
        // Arrange
        var registrationConfirmationId = _fixture.Create<Guid>();
        _registrationConfirmationRepositoryMock.QueryAsync(Arg.Any<Inclusion<Guid, RegistrationConfirmation>[]>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<RegistrationConfirmation>().AsQueryable());

        // Act
        var result = await _sut.GetRegistrationConfirmationWithUserAsync(registrationConfirmationId);

        // Assert
        result.Errors.Should().ContainSingle();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(6)]
    [InlineData(11)]
    [InlineData(27)]
    public async Task GetRegistrationConfirmationWithUserAsync_CallWithMultipleExistingRegistrationConfirmation_Fails(
        int existingRegistrationConfirmationCount)
    {
        // Arrange
        var registrationConfirmationId = _fixture.Create<Guid>();

        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.Id,
                registrationConfirmationId));
        var registrationConfirmations =
            _fixture.CreateMany<RegistrationConfirmation>(existingRegistrationConfirmationCount).AsQueryable();

        _registrationConfirmationRepositoryMock.QueryAsync(Arg.Any<Inclusion<Guid, RegistrationConfirmation>[]>(), Arg.Any<CancellationToken>())
            .Returns(registrationConfirmations);

        // Act
        var result = await _sut.GetRegistrationConfirmationWithUserAsync(registrationConfirmationId);

        // Assert
        result.Failed.Should().BeTrue();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(14)]
    [InlineData(24)]
    public async Task
        GetRegistrationConfirmationWithUserAsync_CallWithMultipleExistingRegistrationConfirmation_ErrorCountIsOne(
            int existingRegistrationConfirmationCount)
    {
        // Arrange
        var registrationConfirmationId = _fixture.Create<Guid>();

        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.Id,
                registrationConfirmationId));
        var registrationConfirmations =
            _fixture.CreateMany<RegistrationConfirmation>(existingRegistrationConfirmationCount).AsQueryable();

        _registrationConfirmationRepositoryMock.QueryAsync(Arg.Any<Inclusion<Guid, RegistrationConfirmation>[]>(), Arg.Any<CancellationToken>())
            .Returns(registrationConfirmations);

        // Act
        var result = await _sut.GetRegistrationConfirmationWithUserAsync(registrationConfirmationId);

        // Assert
        result.Errors.Should().ContainSingle();
    }

    // GetUnconfirmedRegistrationConfirmationsWithUserAsync tests

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
    [InlineData(5, 4)]
    [InlineData(10, 4)]
    [InlineData(25, 4)]
    [InlineData(5, 5)]
    [InlineData(10, 5)]
    [InlineData(25, 5)]
    [InlineData(10, 8)]
    [InlineData(25, 8)]
    [InlineData(10, 10)]
    [InlineData(25, 10)]
    [InlineData(25, 19)]
    [InlineData(25, 25)]
    public async Task GetUnconfirmedRegistrationConfirmationsWithUserAsync_Call_Succeeds(int existingRegistrationConfirmationsCount,
        int unconfirmedRegistrationConfirmationsCount)
    {
        // Arrange
        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.ConfirmationDate,
                _fixture.Create<DateTime>()));
        var existingRegistrations =
            _fixture.CreateMany<RegistrationConfirmation>(existingRegistrationConfirmationsCount).AsQueryable();
        
        // Set random RegistrationConfirmations to unconfirmed
        var random = new Random();
        existingRegistrations.Select(existingRegistration => new { orderKey = random.Next(), existingRegistration})
            .OrderBy(tmp => tmp.orderKey)
            .Take(unconfirmedRegistrationConfirmationsCount)
            .ToList()
            .ForEach(tmp => tmp.existingRegistration.ConfirmationDate = null);
        
        _registrationConfirmationRepositoryMock.QueryAsync(Arg.Any<Inclusion<Guid, RegistrationConfirmation>[]>(), Arg.Any<CancellationToken>())
            .Returns(existingRegistrations);

        // Act
        var result = await _sut.GetUnconfirmedRegistrationConfirmationsWithUserAsync();

        // Assert
        result.Succeeded.Should().BeTrue();
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
    [InlineData(5, 4)]
    [InlineData(10, 4)]
    [InlineData(25, 4)]
    [InlineData(5, 5)]
    [InlineData(10, 5)]
    [InlineData(25, 5)]
    [InlineData(10, 8)]
    [InlineData(25, 8)]
    [InlineData(10, 10)]
    [InlineData(25, 10)]
    [InlineData(25, 19)]
    [InlineData(25, 25)]
    public async Task GetUnconfirmedRegistrationConfirmationsWithUserAsync_Call_ReturnedRegistrationConfirmationsCountEqualsUnconfirmedRegistrationConfirmationsCount(int existingRegistrationConfirmationsCount,
        int unconfirmedRegistrationConfirmationsCount)
    {
        // Arrange
        _fixture.Customize<RegistrationConfirmation>(composerTransformation =>
            composerTransformation.With(registrationConfirmation => registrationConfirmation.ConfirmationDate,
                _fixture.Create<DateTime>()));
        var existingRegistrations =
            _fixture.CreateMany<RegistrationConfirmation>(existingRegistrationConfirmationsCount).AsQueryable();
        
        // Set randomRegistrationConfirmations to unconfirmed
        var random = new Random();
        existingRegistrations.Select(existingRegistration => new { orderKey = random.Next(), existingRegistration})
            .OrderBy(tmp => tmp.orderKey)
            .Take(unconfirmedRegistrationConfirmationsCount)
            .ToList()
            .ForEach(tmp => tmp.existingRegistration.ConfirmationDate = null);
        
        _registrationConfirmationRepositoryMock.QueryAsync(Arg.Any<Inclusion<Guid, RegistrationConfirmation>[]>(), Arg.Any<CancellationToken>())
            .Returns(existingRegistrations);

        // Act
        var result = await _sut.GetUnconfirmedRegistrationConfirmationsWithUserAsync();

        // Assert
        result.Value.Should().HaveCount(unconfirmedRegistrationConfirmationsCount);
    }
}