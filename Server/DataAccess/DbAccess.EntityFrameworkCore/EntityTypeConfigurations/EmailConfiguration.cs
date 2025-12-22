using FlorianAlbert.FinanceObserver.Server.CrossCutting.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.EntityTypeConfigurations;

internal class EmailConfiguration : BaseEntityConfiguration<Guid, Email>
{
    public override void Configure(EntityTypeBuilder<Email> builder)
    {
        base.Configure(builder);
        
        builder.HasMany(email => email.Receivers)
            .WithMany(user => user.Emails);
    }
}