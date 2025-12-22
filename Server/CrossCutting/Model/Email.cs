namespace FlorianAlbert.FinanceObserver.Server.CrossCutting.Model;

public class Email : IBaseEntity<Guid>
{
    public Guid Id { get; set; }

    public required string Subject { get; set; }

    public required string Message { get; set; }

    // Navigation properties
    public ICollection<User> Receivers
    {
        get => field ??= [];
        set;
    }

    public DateTimeOffset CreatedDate { get; set; }

    public DateTimeOffset UpdatedDate { get; set; }
}