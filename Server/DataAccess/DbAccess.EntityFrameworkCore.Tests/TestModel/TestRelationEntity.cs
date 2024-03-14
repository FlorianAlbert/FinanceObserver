using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.Tests.TestModel;

public class TestRelationEntity : BaseEntity<Guid>
{
    private ICollection<TestEntity>? _testEntities;
    public ICollection<TestEntity> TestEntities
    {
        get => _testEntities ??= new List<TestEntity>();
        set => _testEntities = value.ToList();
    }
}