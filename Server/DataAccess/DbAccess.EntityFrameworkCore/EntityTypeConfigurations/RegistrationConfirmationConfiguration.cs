using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.Model;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.EntityTypeConfigurations;

internal class RegistrationConfirmationConfiguration : BaseEntityConfiguration<Guid, RegistrationConfirmation>
{
    public override void Configure(EntityTypeBuilder<RegistrationConfirmation> builder)
    {
        builder.HasOne(registrationConfirmation => registrationConfirmation.User)
            .WithOne(user => user.RegistrationConfirmation)
            .HasForeignKey<RegistrationConfirmation>("UserId");
    }
}