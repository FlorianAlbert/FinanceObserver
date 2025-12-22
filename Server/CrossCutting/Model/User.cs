using Microsoft.AspNetCore.Identity;

namespace FlorianAlbert.FinanceObserver.Server.CrossCutting.Model;

public sealed class User : IdentityUser<Guid>, IBaseEntity<Guid>
{
    public User()
    {
    }

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