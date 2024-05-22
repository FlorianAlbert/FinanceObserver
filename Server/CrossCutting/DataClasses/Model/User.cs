namespace FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.Model;

public sealed class User : BaseEntity<Guid>
{
    public required string UserName { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    public required string EmailAddress { get; set; }

    public required DateOnly BirthDate { get; init; }

    public required string PasswordHash { get; set; }

    // Navigation properties
    private RegistrationConfirmation? _registrationConfirmation;
    public RegistrationConfirmation RegistrationConfirmation
    {
        get => _registrationConfirmation ??= new RegistrationConfirmation
        {
            Id = Guid.Empty,
            User = this
        };
        set => _registrationConfirmation = value;
    }

    private ICollection<Transaction>? _transactions;
    public ICollection<Transaction> Transactions
    {
        get => _transactions ??= [];
        set => _transactions = [.. value];
    }

    private ICollection<Email>? _emails;
    public ICollection<Email> Emails
    {
        get => _emails ??= [];
        set => _emails = [.. value];
    }
}