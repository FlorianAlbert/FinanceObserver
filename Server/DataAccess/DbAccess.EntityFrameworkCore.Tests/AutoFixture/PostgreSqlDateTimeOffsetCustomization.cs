namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.Tests.AutoFixture;

public class PostgreSqlDateTimeOffsetCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customizations.Add(new UtcRandomDateTimeSequenceGenerator());
    }
}
