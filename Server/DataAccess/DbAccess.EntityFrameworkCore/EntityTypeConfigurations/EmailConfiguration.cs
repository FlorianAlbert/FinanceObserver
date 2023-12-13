using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.EntityTypeConfigurations;

public class EmailConfiguration : BaseEntityConfiguration<Guid, Email>
{
    public override void Configure(EntityTypeBuilder<Email> builder)
    {
        base.Configure(builder);
        
        builder.HasMany(email => email.Receivers)
            .WithMany(user => user.Emails);
    }
}