namespace FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.Model;

public class Email : BaseEntity<Guid>
{
    public required string Subject { get; set; }

    public required string Message { get; set; }

    // Navigation properties
    private ICollection<User>? _receivers;
    public ICollection<User> Receivers
    {
        get => _receivers ??= [];
        set => _receivers = [.. value];
    }
}