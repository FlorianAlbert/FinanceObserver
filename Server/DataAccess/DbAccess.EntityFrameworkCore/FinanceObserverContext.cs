using FlorianAlbert.FinanceObserver.Server.CrossCutting.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore;

public class FinanceObserverContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public FinanceObserverContext(DbContextOptions<FinanceObserverContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssemblyReference).Assembly, t => true);
    }
}