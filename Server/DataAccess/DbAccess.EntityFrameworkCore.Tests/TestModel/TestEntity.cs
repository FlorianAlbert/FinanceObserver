using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.Tests.TestModel;

public class TestEntity : BaseEntity<Guid>
{
    // ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength
    public required string Name { get; init; }
    // ReSharper restore EntityFramework.ModelValidation.UnlimitedStringLength
    
    public TestRelationEntity? Relation { get; init; }
}