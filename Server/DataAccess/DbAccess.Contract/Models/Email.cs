namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;

public class Email : BaseEntity<Guid>
{
    public required string Subject { get; set; }

    public required string Message { get; set; }
    
    // Navigation properties
    private ICollection<User>? _receivers;
    public ICollection<User> Receivers
    {
        get => _receivers ??= new List<User>();
        set => _receivers = value.ToList();
    }
}