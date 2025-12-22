using FlorianAlbert.FinanceObserver.Server.CrossCutting.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.EntityTypeConfigurations;

internal class UserConfiguration : BaseEntityConfiguration<Guid, User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasMany(user => user.Transactions)
            .WithOne(transaction => transaction.Owner)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(user => user.Emails)
            .WithMany(email => email.Receivers);
    }
}