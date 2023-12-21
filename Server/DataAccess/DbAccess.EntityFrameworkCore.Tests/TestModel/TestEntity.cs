using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.Tests.TestModel;

public class TestEntity : BaseEntity<Guid>
{
    public TestRelationEntity TestSingleRelation { get; set; }
    
    private ICollection<TestRelationEntity>? _testManyRelation;
    public ICollection<TestRelationEntity> TestManyRelation
    {
        get => _testManyRelation ??= new List<TestRelationEntity>();
        set => _testManyRelation = value.ToList();
    }
}