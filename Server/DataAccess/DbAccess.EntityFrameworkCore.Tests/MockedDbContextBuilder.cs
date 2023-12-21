using System.Diagnostics.CodeAnalysis;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;
using Microsoft.EntityFrameworkCore;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.Tests;

[ExcludeFromCodeCoverage]
public class MockedDbContextBuilder<T>
    where T : DbContext
{
    private readonly T _dbContextMock;
    private readonly Dictionary<Type, IQueryable> _dbSetMocks;
    
    public static MockedDbContextBuilder<T> Create(DbContextOptions<T>? options = null)
    {
        options ??= new DbContextOptions<T>();
        
        return new MockedDbContextBuilder<T>(options);
    }

    private MockedDbContextBuilder(DbContextOptions<T> options)
    {
        _dbContextMock = Substitute.For<T>(options);
        _dbSetMocks = new Dictionary<Type, IQueryable>();
    }

    public MockedDbContextBuilder<T> WithDbSet<TKey, TEntity>(IEnumerable<TEntity> entities)
        where TKey : IParsable<TKey>,
                     IEquatable<TKey>
        where TEntity : BaseEntity<TKey>
    {
        var queryable = entities as IQueryable<TEntity> ?? entities.AsQueryable();
        var mockSet = Substitute.For<DbSet<TEntity>, IQueryable<TEntity>>();

        ((IQueryable<TEntity>)mockSet).Provider.Returns(queryable.Provider);
        ((IQueryable<TEntity>)mockSet).Expression.Returns(queryable.Expression);
        ((IQueryable<TEntity>)mockSet).ElementType.Returns(queryable.ElementType);
        using (var enumerator = ((IQueryable<TEntity>)mockSet).GetEnumerator())
        {
            enumerator.Returns(queryable.GetEnumerator());
        }

        foreach (var entity in queryable)
        {
            mockSet.FindAsync(Arg.Is<object?[]?>(x => x != null && x.Length == 1 && entity.Id.Equals(x[0])), Arg.Any<CancellationToken>()).Returns(entity);
        }

        _dbSetMocks[typeof(TEntity)] = mockSet;

        _dbContextMock.Set<TEntity>().Returns(mockSet);
        
        return this;
    }

    public MockedDbContextBuilder<T> WithObservedDbSet<TKey, TEntity>(ICollection<TEntity> entitiesRef)
        where TKey : IParsable<TKey>,
        IEquatable<TKey>
        where TEntity : BaseEntity<TKey>
    {
        var queryable = entitiesRef.AsQueryable();
        var mockSet = Substitute.For<DbSet<TEntity>, IQueryable<TEntity>>();

        ((IQueryable<TEntity>)mockSet).Provider.Returns(queryable.Provider);
        ((IQueryable<TEntity>)mockSet).Expression.Returns(queryable.Expression);
        ((IQueryable<TEntity>)mockSet).ElementType.Returns(queryable.ElementType);
        using (var enumerator = ((IQueryable<TEntity>)mockSet).GetEnumerator())
        {
            enumerator.Returns(queryable.GetEnumerator());
        }

        foreach (var entity in queryable)
        {
            mockSet.FindAsync(Arg.Is<object?[]?>(x => x != null && x.Length == 1 && entity.Id.Equals(x[0])), Arg.Any<CancellationToken>()).Returns(entity);
        }
        
        mockSet.When(set => set.AddRangeAsync(Arg.Any<IEnumerable<TEntity>>(), Arg.Any<CancellationToken>())).Do(info =>
        {
            var newlyAddedEntities = info.Arg<IEnumerable<TEntity>>();
            if (entitiesRef is List<TEntity> list)
            {
                list.AddRange(newlyAddedEntities);
            }
            else
            {
                foreach (var item in newlyAddedEntities)
                {
                    entitiesRef.Add(item);
                }
            }
        });
        mockSet.When(set => set.AddAsync(Arg.Any<TEntity>())).Do(info => entitiesRef.Add(info.Arg<TEntity>()));
        mockSet.When(set => set.Remove(Arg.Any<TEntity>())).Do(info => entitiesRef.Remove(info.Arg<TEntity>()));
        mockSet.When(set => set.Update(Arg.Any<TEntity>())).Do(info =>
        {
            var updatedEntity = info.Arg<TEntity>();
            entitiesRef.Remove(entitiesRef.Single(e => e.Id.Equals(updatedEntity.Id)));
            entitiesRef.Add(updatedEntity);
        });

        _dbSetMocks[typeof(TEntity)] = mockSet;

        _dbContextMock.Set<TEntity>().Returns(mockSet);
        
        return this;
    }

    public T Build()
    {
        return _dbContextMock;
    }

    public DbSet<TEntity>? GetDbSetMock<TKey, TEntity>()
        where TKey : IParsable<TKey>,
                     IEquatable<TKey>
        where TEntity : BaseEntity<TKey>
    {
        if (_dbSetMocks.TryGetValue(typeof(TEntity), out var mock))
        {
            return mock as DbSet<TEntity>;
        }

        return null;
    }
}