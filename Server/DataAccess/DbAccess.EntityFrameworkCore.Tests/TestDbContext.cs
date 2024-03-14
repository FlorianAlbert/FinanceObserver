using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.Tests.TestModel;
using Microsoft.EntityFrameworkCore;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.Tests;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TestEntity>()
            .HasKey(entity => entity.Id);
        modelBuilder.Entity<TestEntity>()
            .HasOne(entity => entity.Relation)
            .WithMany(relation => relation.TestEntities);
        
        modelBuilder.Entity<TestRelationEntity>()
            .HasKey(entity => entity.Id);
        modelBuilder.Entity<TestRelationEntity>()
            .HasMany(entity => entity.TestEntities)
            .WithOne(relation => relation.Relation);
    }
}