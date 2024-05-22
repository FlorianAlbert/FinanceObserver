using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Diagnostics.CodeAnalysis;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore;

[ExcludeFromCodeCoverage]
public class FinanceObserverContextDesignTimeFactory : IDesignTimeDbContextFactory<FinanceObserverContext>
{
    public FinanceObserverContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FinanceObserverContext>();
        optionsBuilder.UseNpgsql();

        return new FinanceObserverContext(optionsBuilder.Options);
    }
}