using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.Tests.TestModel;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.Tests;

public class RepositoryTests : IAsyncLifetime
{
    private readonly Fixture _fixture;

    private Repository<Guid, TestEntity> _sut = null!;
    
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithCleanUp(true)
        .Build();

    private TestDbContext _context = null!;

    public RepositoryTests()
    {
        _fixture = new Fixture();
    }

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
        
        var contextOptions = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql(_postgreSqlContainer.GetConnectionString())
            .Options;

        _context = new TestDbContext(contextOptions);
        
        _sut = new Repository<Guid, TestEntity>(_context, new InclusionEvaluator());
        
        await _context.Database.MigrateAsync();
    }

    public Task DisposeAsync()
    {
        return _postgreSqlContainer.DisposeAsync().AsTask();
    }

    [Fact]
    public async Task QueryAsync_Call_ReturnsQueryableReturnedByInclusionEvaluator()
    {
        // Arrange
        var testEntities = _fixture.CreateMany<TestEntity>().AsQueryable();

        await _context.AddRangeAsync(testEntities);
        await _context.SaveChangesAsync();
        
        _context.ChangeTracker.Clear();

        // Act
        var result = await _sut.QueryAsync();

        // Assert
        result.Should().BeEquivalentTo(testEntities);
    }

    [Fact]
    public async Task FindAsync_CallWithExistingEntity_Succeeds()
    {
        // Arrange
        var testEntity = _fixture.Create<TestEntity>();

        await _context.AddAsync(testEntity);
        await _context.SaveChangesAsync();
        
        _context.ChangeTracker.Clear();

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

        await _context.AddAsync(testEntity);
        await _context.SaveChangesAsync();
        
        _context.ChangeTracker.Clear();

        // Act
        var result = await _sut.FindAsync(testEntity.Id);

        // Assert
        result.Value.Should().BeEquivalentTo(testEntity);
    }

    [Fact]
    public async Task FindAsync_CallWithNonExistingEntity_Fails()
    {
        // Arrange

        // Act
        var result = await _sut.FindAsync(Guid.NewGuid());

        // Assert
        result.Failed.Should().BeTrue();
    }

    [Fact]
    public async Task FindAsync_CallWithNonExistingEntity_ReturnsOneError()
    {
        // Arrange

        // Act
        var result = await _sut.FindAsync(Guid.NewGuid());

        // Assert
        result.Errors.Should().ContainSingle();
    }

    [Fact]
    public async Task DeleteAsync_CallWithExistingEntityInEnumerable_DbCommandMockCalled()
    {
        // Arrange
        var entityToDelete = _fixture.Create<TestEntity>();

        await _context.AddAsync(entityToDelete);
        await _context.SaveChangesAsync();
        
        _context.ChangeTracker.Clear();

        // Act
        await _sut.DeleteAsync([entityToDelete]);

        // Assert
        var notFoundEntity = await _context.FindAsync<TestEntity>(entityToDelete.Id);
        notFoundEntity.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_CallWithExistingEntity_DbCommandMockCalled()
    {
        // Arrange
        var entityToDelete = _fixture.Create<TestEntity>();
        var stillExistentEntities = _fixture.CreateMany<TestEntity>().ToList();

        var storedEntities = stillExistentEntities.Concat([entityToDelete]);
        
        await _context.AddRangeAsync(storedEntities);
        await _context.SaveChangesAsync();
        
        _context.ChangeTracker.Clear();

        // Act
        await _sut.DeleteAsync(entityToDelete);

        // Assert
        foreach (var entity in stillExistentEntities)
        {
            var foundEntity = await _context.FindAsync<TestEntity>(entity.Id);
            foundEntity.Should().NotBeNull();
        }
        
        var notFoundEntity = await _context.FindAsync<TestEntity>(entityToDelete.Id);
        notFoundEntity.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_CallWithExistingKey_DbCommandMockCalled()
    {
        // Arrange
        var entityToDelete = _fixture.Create<TestEntity>();
        var stillExistentEntities = _fixture.CreateMany<TestEntity>().ToList();

        var storedEntities = stillExistentEntities.Concat([entityToDelete]);
        
        await _context.AddRangeAsync(storedEntities);
        await _context.SaveChangesAsync();
        
        _context.ChangeTracker.Clear();

        // Act
        await _sut.DeleteAsync(entityToDelete.Id);

        // Assert
        foreach (var entity in stillExistentEntities)
        {
            var foundEntity = await _context.FindAsync<TestEntity>(entity.Id);
            foundEntity.Should().NotBeNull();
        }
        
        var notFoundEntity = await _context.FindAsync<TestEntity>(entityToDelete.Id);
        notFoundEntity.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_CallWithValidExpression_DbCommandMockCalled()
    {
        // Arrange
        var entityToDelete = _fixture.Create<TestEntity>();
        var stillExistentEntities = _fixture.CreateMany<TestEntity>().ToList();

        var storedEntities = stillExistentEntities.Concat([entityToDelete]);
        
        await _context.AddRangeAsync(storedEntities);
        await _context.SaveChangesAsync();
        
        _context.ChangeTracker.Clear();

        // Act
        await _sut.DeleteAsync(entity => entity.Id == entityToDelete.Id);

        // Assert
        foreach (var entity in stillExistentEntities)
        {
            var foundEntity = await _context.FindAsync<TestEntity>(entity.Id);
            foundEntity.Should().NotBeNull();
        }
        
        var notFoundEntity = await _context.FindAsync<TestEntity>(entityToDelete.Id);
        notFoundEntity.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_CallWithInvalidExpression_DbCommandMockCalledWithEmptyQueryable()
    {
        // Arrange
        var stillExistentEntities = _fixture.CreateMany<TestEntity>().ToList();
        
        await _context.AddRangeAsync(stillExistentEntities);
        await _context.SaveChangesAsync();
        
        _context.ChangeTracker.Clear();

        // Act
        await _sut.DeleteAsync(entity => false);

        // Assert
        foreach (var entity in stillExistentEntities)
        {
            var foundEntity = await _context.FindAsync<TestEntity>(entity.Id);
            foundEntity.Should().NotBeNull();
        }
    }
}