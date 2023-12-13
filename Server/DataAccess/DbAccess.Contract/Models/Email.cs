namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;

public class Email : BaseEntity<Guid>
{
    // Navigation properties
    private ICollection<User>? _receivers;
    public required string Subject { get; set; }

    public required string Message { get; set; }

    public ICollection<User> Receivers
    {
        get => _receivers ??= new List<User>();
        set => _receivers = value.ToList();
    }
}