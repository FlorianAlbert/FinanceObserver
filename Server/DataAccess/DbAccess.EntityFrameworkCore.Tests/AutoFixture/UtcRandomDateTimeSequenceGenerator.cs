using AutoFixture.Kernel;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.Tests.AutoFixture;

public class UtcRandomDateTimeSequenceGenerator : ISpecimenBuilder
{
    private readonly RandomDateTimeSequenceGenerator _innerBuilder;

    public UtcRandomDateTimeSequenceGenerator()
    {
        _innerBuilder = new RandomDateTimeSequenceGenerator();
    }

    public object Create(object request, ISpecimenContext context)
    {
        object result = _innerBuilder.Create(request, context);

        if (result is NoSpecimen or OmitSpecimen)
        {
            return result;
        }

        if (result is DateTime dateTime)
        {
            var utcDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            long ticks = utcDateTime.Ticks;
            // PostgreSQL stores timestamps with microsecond precision (1 microsecond = 10 ticks)
            long truncatedTicks = ticks / 10 * 10;
            return new DateTime(truncatedTicks, DateTimeKind.Utc);
        }

        return result;
    }
}
