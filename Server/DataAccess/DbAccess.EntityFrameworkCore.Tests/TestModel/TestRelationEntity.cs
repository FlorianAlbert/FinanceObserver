using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.Model;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.Tests.TestModel;

public class TestRelationEntity : BaseEntity<Guid>
{
    private ICollection<TestEntity>? _testEntities;
    public ICollection<TestEntity> TestEntities
    {
        get => _testEntities ??= [];
        set => _testEntities = [.. value];
    }
}