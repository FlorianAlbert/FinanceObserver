namespace FlorianAlbert.FinanceObserver.Server.CrossCutting.Model;

public class Transaction : IBaseEntity<Guid>
{
    public Guid Id { get; set; }

    public required string Title { get; set; }

    public required string Description { get; set; }

    public required Currency Currency { get; set; }

    public required decimal Amount { get; set; }

    public required DateTimeOffset Time { get; set; }

    public required User Owner { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public DateTimeOffset UpdatedDate { get; set; }
}