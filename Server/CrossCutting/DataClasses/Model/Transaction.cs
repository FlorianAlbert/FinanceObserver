namespace FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.Model;

public class Transaction : BaseEntity<Guid>
{
    public required string Title { get; set; }

    public required string Description { get; set; }

    public required Currency Currency { get; set; }

    public required decimal Amount { get; set; }

    public required DateTimeOffset Time { get; set; }

    public required User Owner { get; set; }
}