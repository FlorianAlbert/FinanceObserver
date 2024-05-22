using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.Model;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.Tests.TestModel;

public class TestEntity : BaseEntity<Guid>
{
    // ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength
    public required string Name { get; init; }
    // ReSharper restore EntityFramework.ModelValidation.UnlimitedStringLength

    public TestRelationEntity? Relation { get; init; }
}