using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.InfrastructureTypes;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Data;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.Tests.AutoFixture;
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
        _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _fixture.Customize(new PostgreSqlDateTimeOffsetCustomization());
    }

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        DbContextOptions<TestDbContext> contextOptions = new DbContextOptionsBuilder<TestDbContext>()
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
        _fixture.Customize<Guid>(c => c.FromFactory(() => Guid.Empty));
        IQueryable<TestEntity> testEntities = _fixture.CreateMany<TestEntity>().AsQueryable();

        await _context.AddRangeAsync(testEntities);
        await _context.SaveChangesAsync();
        
        _context.ChangeTracker.Clear();

        // Act
        IQueryable<TestEntity> result = await _sut.QueryAsync();

        // Assert
        result.Should().BeEquivalentTo(testEntities, options => options.Excluding(entity => entity.Relation));
    }
    
    [Fact]
    public async Task QueryAsync_CallWithoutIncludes_ReturnsQueryableWithoutRelationsIncluded()
    {
        // Arrange
        _fixture.Customize<Guid>(c => c.FromFactory(() => Guid.Empty));
        IEnumerable<TestEntity> testEntities = _fixture.CreateMany<TestEntity>();

        await _context.AddRangeAsync(testEntities);
        await _context.SaveChangesAsync();
        
        _context.ChangeTracker.Clear();

        // Act
        IQueryable<TestEntity> result = await _sut.QueryAsync();

        // Assert
        foreach (TestEntity entity in result)
        {
            entity.Relation.Should().BeNull();
        }
    }
    
    [Fact]
    public async Task QueryAsync_CallWithIncludes_ReturnsQueryableWithRelationsIncluded()
    {
        // Arrange
        _fixture.Customize<Guid>(c => c.FromFactory(() => Guid.Empty));
        IEnumerable<TestEntity> testEntities = _fixture.CreateMany<TestEntity>();

        await _context.AddRangeAsync(testEntities);
        await _context.SaveChangesAsync();
        
        _context.ChangeTracker.Clear();

        // Act
        IQueryable<TestEntity> result = await _sut.QueryAsync([Inclusion<Guid, TestEntity>.Of<Guid, TestRelationEntity?>(entity => entity.Relation)]);

        // Assert
        foreach (TestEntity entity in result)
        {
            entity.Relation.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task FindAsync_CallWithExistingEntity_Succeeds()
    {
        // Arrange
        TestEntity testEntity = _fixture.Create<TestEntity>();

        await _context.AddAsync(testEntity);
        await _context.SaveChangesAsync();
        
        _context.ChangeTracker.Clear();

        // Act
        Result<TestEntity> result = await _sut.FindAsync(testEntity.Id);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task FindAsync_CallWithExistingEntity_ReturnsEntity()
    {
        // Arrange
        TestEntity testEntity = _fixture.Create<TestEntity>();

        await _context.AddAsync(testEntity);
        await _context.SaveChangesAsync();
        
        _context.ChangeTracker.Clear();

        // Act
        Result<TestEntity> result = await _sut.FindAsync(testEntity.Id);

        // Assert
        result.Value.Should().BeEquivalentTo(testEntity, options => options.Excluding(entity => entity.Relation));
    }

    [Fact]
    public async Task FindAsync_CallWithNonExistingEntity_Fails()
    {
        // Arrange

        // Act
        Result<TestEntity> result = await _sut.FindAsync(Guid.NewGuid());

        // Assert
        result.Failed.Should().BeTrue();
    }

    [Fact]
    public async Task FindAsync_CallWithNonExistingEntity_ReturnsOneError()
    {
        // Arrange

        // Act
        Result<TestEntity> result = await _sut.FindAsync(Guid.NewGuid());

        // Assert
        result.Errors.Should().ContainSingle();
    }

    [Fact]
    public async Task DeleteAsync_CallWithExistingEntity_EntityNotFoundAfterDeletion()
    {
        // Arrange
        TestEntity entityToDelete = _fixture.Create<TestEntity>();

        await _context.AddAsync(entityToDelete);
        await _context.SaveChangesAsync();
        
        _context.ChangeTracker.Clear();

        // Act
        await _sut.DeleteAsync([entityToDelete]);

        // Assert
        TestEntity? notFoundEntity = await _context.FindAsync<TestEntity>(entityToDelete.Id);
        notFoundEntity.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_CallWithExistingEntityInEnumerable_EntityNotFoundAfterDeletionAndOthersStillExistent()
    {
        // Arrange
        TestEntity entityToDelete = _fixture.Create<TestEntity>();
        List<TestEntity> stillExistentEntities = [.. _fixture.CreateMany<TestEntity>()];

        IEnumerable<TestEntity> storedEntities = stillExistentEntities.Concat([entityToDelete]);
        
        await _context.AddRangeAsync(storedEntities);
        await _context.SaveChangesAsync();
        
        _context.ChangeTracker.Clear();

        // Act
        await _sut.DeleteAsync(entityToDelete);

        // Assert
        foreach (TestEntity entity in stillExistentEntities)
        {
            TestEntity? foundEntity = await _context.FindAsync<TestEntity>(entity.Id);
            foundEntity.Should().NotBeNull();
        }

        TestEntity? notFoundEntity = await _context.FindAsync<TestEntity>(entityToDelete.Id);
        notFoundEntity.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_CallWithExistingKey_EntityNotFoundAfterDeletionAndOthersStillExistent()
    {
        // Arrange
        TestEntity entityToDelete = _fixture.Create<TestEntity>();
        List<TestEntity> stillExistentEntities = [.. _fixture.CreateMany<TestEntity>()];

        IEnumerable<TestEntity> storedEntities = stillExistentEntities.Concat([entityToDelete]);
        
        await _context.AddRangeAsync(storedEntities);
        await _context.SaveChangesAsync();
        
        _context.ChangeTracker.Clear();

        // Act
        await _sut.DeleteAsync(entityToDelete.Id);

        // Assert
        foreach (TestEntity entity in stillExistentEntities)
        {
            TestEntity? foundEntity = await _context.FindAsync<TestEntity>(entity.Id);
            foundEntity.Should().NotBeNull();
        }

        TestEntity? notFoundEntity = await _context.FindAsync<TestEntity>(entityToDelete.Id);
        notFoundEntity.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_CallWithValidExpression_EntityNotFoundAfterDeletionAndOthersStillExistent()
    {
        // Arrange
        TestEntity entityToDelete = _fixture.Create<TestEntity>();
        List<TestEntity> stillExistentEntities = [.. _fixture.CreateMany<TestEntity>()];

        IEnumerable<TestEntity> storedEntities = stillExistentEntities.Concat([entityToDelete]);
        
        await _context.AddRangeAsync(storedEntities);
        await _context.SaveChangesAsync();
        
        _context.ChangeTracker.Clear();

        // Act
        await _sut.DeleteAsync(entity => entity.Id == entityToDelete.Id);

        // Assert
        foreach (TestEntity entity in stillExistentEntities)
        {
            TestEntity? foundEntity = await _context.FindAsync<TestEntity>(entity.Id);
            foundEntity.Should().NotBeNull();
        }

        TestEntity? notFoundEntity = await _context.FindAsync<TestEntity>(entityToDelete.Id);
        notFoundEntity.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_CallWithInvalidExpression_AllEntitiesStillExistent()
    {
        // Arrange
        List<TestEntity> stillExistentEntities = [.. _fixture.CreateMany<TestEntity>()];
        
        await _context.AddRangeAsync(stillExistentEntities);
        await _context.SaveChangesAsync();
        
        _context.ChangeTracker.Clear();

        // Act
        await _sut.DeleteAsync(entity => false);

        // Assert
        foreach (TestEntity entity in stillExistentEntities)
        {
            TestEntity? foundEntity = await _context.FindAsync<TestEntity>(entity.Id);
            foundEntity.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task ExistsAsync_CallWithValidExpression_ReturnsTrue()
    {
        // Arrange
        TestEntity existingEntity = _fixture.Create<TestEntity>();
        
        await _context.AddAsync(existingEntity);
        await _context.SaveChangesAsync();
        
        _context.ChangeTracker.Clear();

        // Act
        bool result = await _sut.ExistsAsync(entity => entity.Id.Equals(existingEntity.Id));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_CallWithInvalidExpression_ReturnsFalse()
    {
        // Arrange

        // Act
        bool result = await _sut.ExistsAsync(entity => false);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_CallWithExistingId_ReturnsTrue()
    {
        // Arrange
        TestEntity existingEntity = _fixture.Create<TestEntity>();
        
        await _context.AddAsync(existingEntity);
        await _context.SaveChangesAsync();
        
        _context.ChangeTracker.Clear();

        // Act
        bool result = await _sut.ExistsAsync(existingEntity.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_CallWithNonExistingId_ReturnsFalse()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        bool result = await _sut.ExistsAsync(nonExistingId);

        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task InsertAsync_CallWithNewEntity_EntityInserted()
    {
        // Arrange
        _fixture.Customize<Guid>(c => c.FromFactory(() => Guid.Empty));
        TestEntity entityToInsert = _fixture.Create<TestEntity>();
        
        // Act
        await _sut.InsertAsync(entityToInsert);

        // Assert
        TestEntity? insertedEntity = await _context.FindAsync<TestEntity>(entityToInsert.Id);
        insertedEntity.Should().NotBeNull();
    }
    
    [Fact]
    public async Task InsertAsync_CallWithExistingEntity_ThrowsDbUpdateException()
    {
        // Arrange
        _fixture.Customize<Guid>(c => c.FromFactory(() => Guid.Empty));
        TestEntity existingEntity = _fixture.Create<TestEntity>();
        
        await _context.AddAsync(existingEntity);
        await _context.SaveChangesAsync();
        
        _context.ChangeTracker.Clear();

        // Act
        Func<Task<TestEntity>> act = async () => await _sut.InsertAsync(existingEntity);

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }
    
    [Fact]
    public async Task InsertAsync_CallWithNewEntity_ReturnsInsertedEntity()
    {
        // Arrange
        _fixture.Customize<Guid>(c => c.FromFactory(() => Guid.Empty));
        TestEntity entityToInsert = _fixture.Create<TestEntity>();

        // Act
        TestEntity result = await _sut.InsertAsync(entityToInsert);

        // Assert
        result.Should().BeEquivalentTo(entityToInsert);
    }
    
    [Fact]
    public async Task InsertAsync_CallWithNewEntity_CreatedIsBetweenNowAndBeforeInserted()
    {
        // Arrange
        _fixture.Customize<Guid>(c => c.FromFactory(() => Guid.Empty));
        TestEntity entityToInsert = _fixture.Create<TestEntity>();

        // Act
        DateTimeOffset beforeInsert = DateTimeOffset.UtcNow;
        await _sut.InsertAsync(entityToInsert);
        DateTimeOffset afterInsert = DateTimeOffset.UtcNow;

        // Assert
        entityToInsert.CreatedDate.Should().BeOnOrAfter(beforeInsert);
        entityToInsert.CreatedDate.Should().BeOnOrBefore(afterInsert);
    }
    
    [Fact]
    public async Task InsertAsync_CallWithNewEntity_UpdatedIsEqualToCreated()
    {
        // Arrange
        _fixture.Customize<Guid>(c => c.FromFactory(() => Guid.Empty));
        TestEntity entityToInsert = _fixture.Create<TestEntity>();
        
        // Act
        await _sut.InsertAsync(entityToInsert);

        // Assert
        entityToInsert.UpdatedDate.Should().Be(entityToInsert.CreatedDate);
    }
    
    [Fact]
    public async Task InsertAsync_CallWithNewEntities_EntitiesInserted()
    {
        // Arrange
        _fixture.Customize<Guid>(c => c.FromFactory(() => Guid.Empty));
        List<TestEntity> entitiesToInsert = [.. _fixture.CreateMany<TestEntity>()];
        
        // Act
        await _sut.InsertAsync(entitiesToInsert);

        // Assert
        foreach (TestEntity entity in entitiesToInsert)
        {
            TestEntity? insertedEntity = await _context.FindAsync<TestEntity>(entity.Id);
            insertedEntity.Should().NotBeNull();
        }
    }
    
    [Fact]
    public async Task InsertAsync_CallWithExistingEntities_ThrowsDbUpdateException()
    {
        // Arrange
        _fixture.Customize<Guid>(c => c.FromFactory(() => Guid.Empty));
        List<TestEntity> existingEntities = [.. _fixture.CreateMany<TestEntity>()];
        
        await _context.AddRangeAsync(existingEntities);
        await _context.SaveChangesAsync();
        
        _context.ChangeTracker.Clear();

        // Act
        Func<Task> act = async () => await _sut.InsertAsync(existingEntities);

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }
    
    [Fact]
    public async Task InsertAsync_CallWithNewEntities_AllCreatedDatesAreBetweenNowAndBeforeInserted()
    {
        // Arrange
        _fixture.Customize<Guid>(c => c.FromFactory(() => Guid.Empty));
        List<TestEntity> entitiesToInsert = [.. _fixture.CreateMany<TestEntity>()];

        // Act
        DateTimeOffset beforeInsert = DateTimeOffset.UtcNow;
        await _sut.InsertAsync(entitiesToInsert);
        DateTimeOffset afterInsert = DateTimeOffset.UtcNow;

        // Assert
        foreach (TestEntity entity in entitiesToInsert)
        {
            entity.CreatedDate.Should().BeOnOrAfter(beforeInsert);
            entity.CreatedDate.Should().BeOnOrBefore(afterInsert);
        }
    }
    
    [Fact]
    public async Task InsertAsync_CallWithNewEntities_AllCreatedDatesAreEqualToEachOther()
    {
        // Arrange
        _fixture.Customize<Guid>(c => c.FromFactory(() => Guid.Empty));
        List<TestEntity> entitiesToInsert = [.. _fixture.CreateMany<TestEntity>()];
        
        // Act
        await _sut.InsertAsync(entitiesToInsert);

        // Assert
        IEnumerable<DateTimeOffset> createdDates = entitiesToInsert.Select(entity => entity.CreatedDate).Distinct();
        createdDates.Should().ContainSingle();
    }
    
    [Fact]
    public async Task InsertAsync_CallWithNewEntities_AllUpdatedDatesAreEqualToCreated()
    {
        // Arrange
        _fixture.Customize<Guid>(c => c.FromFactory(() => Guid.Empty));
        List<TestEntity> entitiesToInsert = [.. _fixture.CreateMany<TestEntity>()];
        
        // Act
        await _sut.InsertAsync(entitiesToInsert);

        // Assert
        foreach (TestEntity entity in entitiesToInsert)
        {
            entity.UpdatedDate.Should().Be(entity.CreatedDate);
        }
    }
    
    [Fact]
    public async Task UpdateAsync_CallWithExistingEntityAndUpdates_UpdatesAreApplied()
    {
        // Arrange
        TestEntity entityToUpdate = _fixture.Create<TestEntity>();
        
        await _context.AddAsync(entityToUpdate);
        await _context.SaveChangesAsync();
        
        _context.ChangeTracker.Clear();

        Update<TestEntity>[] updates =
        [
            Update<TestEntity>.With(entity => entity.Name, "new name")
        ];
        
        // Act
        await _sut.UpdateAsync(entityToUpdate, updates);

        // Assert
        TestEntity? updatedEntity = await _context.FindAsync<TestEntity>(entityToUpdate.Id);

        updatedEntity.Should().NotBeNull();
        updatedEntity!.Name.Should().Be("new name");
    }
}