namespace FlorianAlbert.FinanceObserver.Server.CrossCutting.Model;

public sealed class User : IBaseEntity<Guid>
{
    public Guid Id { get; set; }

    public required string ExternalId { get; set; }

    public string? Email { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    public DateOnly BirthDate { get; init; }

    // Navigation properties

    public ICollection<Transaction> Transactions
    {
        get => field ??= [];
        set;
    }
    public ICollection<Email> Emails
    {
        get => field ??= [];
        set;
    }

    public DateTimeOffset CreatedDate { get; set; }

    public DateTimeOffset UpdatedDate { get; set; }
}