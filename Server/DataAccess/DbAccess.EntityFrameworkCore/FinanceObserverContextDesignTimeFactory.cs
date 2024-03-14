using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

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