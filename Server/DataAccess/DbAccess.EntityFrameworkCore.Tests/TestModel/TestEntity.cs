using FlorianAlbert.FinanceObserver.Server.CrossCutting.Model;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.Tests.TestModel;

public class TestEntity : IBaseEntity<Guid>
{
    public Guid Id { get; set; }

    public required string Name { get; init; }
    
    public TestRelationEntity? Relation { get; init; }

    public DateTimeOffset CreatedDate { get; set; }

    public DateTimeOffset UpdatedDate { get; set; }
}