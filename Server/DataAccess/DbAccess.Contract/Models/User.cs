namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;

public sealed class User : BaseEntity<Guid>
{
    private ICollection<Email>? _emails;

    // Navigation properties
    private RegistrationConfirmation? _registrationConfirmation;

    private ICollection<Transaction>? _transactions;
    public required string UserName { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    public required string EmailAddress { get; set; }

    public required DateOnly BirthDate { get; init; }

    public required string PasswordHash { get; set; }

    public RegistrationConfirmation RegistrationConfirmation
    {
        get => _registrationConfirmation ??= new RegistrationConfirmation
        {
            Id = Guid.Empty,
            User = this
        };
        set => _registrationConfirmation = value;
    }

    public ICollection<Transaction> Transactions
    {
        get => _transactions ??= new List<Transaction>();
        set => _transactions = value.ToList();
    }

    public ICollection<Email> Emails
    {
        get => _emails ??= new List<Email>();
        set => _emails = value.ToList();
    }
}