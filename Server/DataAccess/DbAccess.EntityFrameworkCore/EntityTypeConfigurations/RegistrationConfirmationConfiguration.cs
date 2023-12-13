using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.EntityTypeConfigurations;

public class RegistrationConfirmationConfiguration : BaseEntityConfiguration<Guid, RegistrationConfirmation>
{
    public override void Configure(EntityTypeBuilder<RegistrationConfirmation> builder)
    {
        builder.HasOne(registrationConfirmation => registrationConfirmation.User)
            .WithOne(user => user.RegistrationConfirmation)
            .HasForeignKey<RegistrationConfirmation>("UserId");
    }
}