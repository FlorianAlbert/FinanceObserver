using FlorianAlbert.FinanceObserver.Server.CrossCutting.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.EntityTypeConfigurations;

internal class TransactionConfiguration : BaseEntityConfiguration<Guid, Transaction>
{
    public override void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasOne(transaction => transaction.Owner)
            .WithMany(user => user.Transactions)
            .OnDelete(DeleteBehavior.Cascade);
    }
}