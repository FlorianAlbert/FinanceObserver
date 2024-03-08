using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore;

public class FinanceObserverContextDesignTimeFactory : IDesignTimeDbContextFactory<FinanceObserverContext>
{
    public FinanceObserverContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FinanceObserverContext>();
        optionsBuilder.UseNpgsql();

        return new FinanceObserverContext(optionsBuilder.Options);
    }
}