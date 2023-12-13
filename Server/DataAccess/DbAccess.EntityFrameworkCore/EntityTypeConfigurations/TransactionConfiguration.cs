using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.EntityTypeConfigurations;

public class TransactionConfiguration : BaseEntityConfiguration<Guid, Transaction>
{
    public override void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasOne(transaction => transaction.Owner)
            .WithMany(user => user.Transactions)
            .OnDelete(DeleteBehavior.Cascade);
    }
}