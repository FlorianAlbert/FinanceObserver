namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;

public class RegistrationConfirmation : BaseEntity<Guid>
{
    public DateTimeOffset? ConfirmationDate { get; set; }

    public bool RegistrationIsConfirmed => ConfirmationDate is not null;

    // Navigation properties
    public required User User { get; set; }
}