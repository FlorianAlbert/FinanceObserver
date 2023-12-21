using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Data;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.Tests.TestModel;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.Tests;

public class RepositoryTests
{
    private readonly Fixture _fixture;

    private readonly Func<Repository<Guid, TestEntity>> _repositoryFactory;

    private readonly MockedDbContextBuilder<FinanceObserverContext> _mockedDbContextBuilder;
    private readonly IInclusionEvaluator _inclusionEvaluatorMock;

    private Repository<Guid, TestEntity>? _sut;

    public RepositoryTests()
    {
        _fixture = new Fixture();

        _mockedDbContextBuilder = MockedDbContextBuilder<FinanceObserverContext>.Create();
        _inclusionEvaluatorMock = Substitute.For<IInclusionEvaluator>();

        _repositoryFactory = () =>
            new Repository<Guid, TestEntity>(_mockedDbContextBuilder.Build(), _inclusionEvaluatorMock);
    }

    [Fact]
    public async Task QueryAsync_CallWithoutInclusions_ReturnsQueryableReturnedByInclusionEvaluator()
    {
        // Arrange
        var testEntities = _fixture.CreateMany<TestEntity>().AsQueryable();
        var evaluatedTestEntities = _fixture.CreateMany<TestEntity>().AsQueryable();
        _mockedDbContextBuilder.WithDbSet<Guid, TestEntity>(testEntities);

        _inclusionEvaluatorMock.Evaluate(
            Arg.Is<IQueryable<TestEntity>>(queryableArg =>
                queryableArg.Count() == testEntities.Count() &&
                queryableArg.All(testEntity => testEntities.Contains(testEntity))),
            Arg.Is<Inclusion<Guid, TestEntity>[]>(inclusions => inclusions.Length == 0),
            Arg.Any<CancellationToken>()).Returns(evaluatedTestEntities);

        _sut = _repositoryFactory();

        // Act
        var result = await _sut.QueryAsync();

        // Assert
        result.Should().BeSameAs(evaluatedTestEntities);
    }

    [Fact]
    public async Task QueryAsync_CallWithSingleRelationInclusion_ReturnsQueryableReturnedByInclusionEvaluator()
    {
        // Arrange
        var testEntities = _fixture.CreateMany<TestEntity>().AsQueryable();
        var evaluatedTestEntities = _fixture.CreateMany<TestEntity>().AsQueryable();
        _mockedDbContextBuilder.WithDbSet<Guid, TestEntity>(testEntities);

        var inclusion = Inclusion<Guid, TestEntity>.Of(e => e.TestSingleRelation);

        _inclusionEvaluatorMock.Evaluate(
            Arg.Is<IQueryable<TestEntity>>(queryableArg =>
                queryableArg.Count() == testEntities.Count() &&
                queryableArg.All(testEntity => testEntities.Contains(testEntity))),
            Arg.Is<Inclusion<Guid, TestEntity>[]>(inclusions => inclusions.Length == 1 && inclusions[0] == inclusion),
            Arg.Any<CancellationToken>()).Returns(evaluatedTestEntities);

        _sut = _repositoryFactory();

        // Act
        var result = await _sut.QueryAsync([inclusion]);

        // Assert
        result.Should().BeSameAs(evaluatedTestEntities);
    }

    [Fact]
    public async Task QueryAsync_CallWithManyRelationInclusion_ReturnsQueryableReturnedByInclusionEvaluator()
    {
        // Arrange
        var testEntities = _fixture.CreateMany<TestEntity>().AsQueryable();
        var evaluatedTestEntities = _fixture.CreateMany<TestEntity>().AsQueryable();
        _mockedDbContextBuilder.WithDbSet<Guid, TestEntity>(testEntities);

        var inclusion = Inclusion<Guid, TestEntity>.Of(e => e.TestManyRelation);

        _inclusionEvaluatorMock.Evaluate(
            Arg.Is<IQueryable<TestEntity>>(queryableArg =>
                queryableArg.Count() == testEntities.Count() &&
                queryableArg.All(testEntity => testEntities.Contains(testEntity))),
            Arg.Is<Inclusion<Guid, TestEntity>[]>(inclusions => inclusions.Length == 1 && inclusions[0] == inclusion),
            Arg.Any<CancellationToken>()).Returns(evaluatedTestEntities);

        _sut = _repositoryFactory();

        // Act
        var result = await _sut.QueryAsync([inclusion]);

        // Assert
        result.Should().BeSameAs(evaluatedTestEntities);
    }

    [Fact]
    public async Task QueryAsync_CallWithMultipleInclusions_ReturnsQueryableReturnedByInclusionEvaluator()
    {
        // Arrange
        var testEntities = _fixture.CreateMany<TestEntity>().AsQueryable();
        var evaluatedTestEntities = _fixture.CreateMany<TestEntity>().AsQueryable();
        _mockedDbContextBuilder.WithDbSet<Guid, TestEntity>(testEntities);

        Inclusion<Guid, TestEntity>[] inclusions =
        [
            Inclusion<Guid, TestEntity>.Of(e => e.TestSingleRelation),
            Inclusion<Guid, TestEntity>.Of(e => e.TestManyRelation)
        ];

        _inclusionEvaluatorMock.Evaluate(
            Arg.Is<IQueryable<TestEntity>>(queryableArg =>
                queryableArg.Count() == testEntities.Count() &&
                queryableArg.All(testEntity => testEntities.Contains(testEntity))),
            Arg.Is<Inclusion<Guid, TestEntity>[]>(passedInclusions => passedInclusions == inclusions),
            Arg.Any<CancellationToken>()).Returns(evaluatedTestEntities);

        _sut = _repositoryFactory();

        // Act
        var result = await _sut.QueryAsync(inclusions);

        // Assert
        result.Should().BeSameAs(evaluatedTestEntities);
    }

    [Fact]
    public async Task FindAsync_CallWithExistingEntity_Succeeds()
    {
        // Arrange
        var testEntity = _fixture.Create<TestEntity>();
        _mockedDbContextBuilder.WithDbSet<Guid, TestEntity>([testEntity]);

        _sut = _repositoryFactory();

        // Act
        var result = await _sut.FindAsync(testEntity.Id);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task FindAsync_CallWithExistingEntity_ReturnsEntity()
    {
        // Arrange
        var testEntity = _fixture.Create<TestEntity>();
        _mockedDbContextBuilder.WithDbSet<Guid, TestEntity>([testEntity]);

        _sut = _repositoryFactory();

        // Act
        var result = await _sut.FindAsync(testEntity.Id);

        // Assert
        result.Value.Should().BeSameAs(testEntity);
    }

    [Fact]
    public async Task FindAsync_CallWithNonExistingEntity_Fails()
    {
        // Arrange
        _mockedDbContextBuilder.WithDbSet<Guid, TestEntity>([]);

        _sut = _repositoryFactory();

        // Act
        var result = await _sut.FindAsync(Guid.NewGuid());

        // Assert
        result.Failed.Should().BeTrue();
    }

    [Fact]
    public async Task FindAsync_CallWithNonExistingEntity_ReturnsOneError()
    {
        // Arrange
        _mockedDbContextBuilder.WithDbSet<Guid, TestEntity>([]);

        _sut = _repositoryFactory();

        // Act
        var result = await _sut.FindAsync(Guid.NewGuid());

        // Assert
        result.Errors.Should().ContainSingle();
    }
}