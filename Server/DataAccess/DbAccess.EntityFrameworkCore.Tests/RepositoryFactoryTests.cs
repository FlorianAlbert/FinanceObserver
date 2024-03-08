using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.Tests.TestModel;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.Tests;

public class RepositoryFactoryTests : IAsyncLifetime
{
    private RepositoryFactory _sut = null!;

    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithCleanUp(true)
        .Build();
    
    private TestDbContext _context = null!;

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
        
        var contextOptions = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql(_postgreSqlContainer.GetConnectionString())
            .Options;

        _context = new TestDbContext(contextOptions);
        
        _sut = new RepositoryFactory(_context);
        
        await _context.Database.MigrateAsync();
    }

    public Task DisposeAsync()
    {
        return _postgreSqlContainer.DisposeAsync().AsTask();
    }
    
    [Fact]
    public void CreateRepository_Call_ReturnedRepositoryIsNotNull()
    {
        // Arrange

        // Act
        var repository = _sut.CreateRepository<Guid, TestEntity>();

        // Assert
        repository.Should().NotBeNull();
    }
}