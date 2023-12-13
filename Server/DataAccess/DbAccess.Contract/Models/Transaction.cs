namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;

public class Transaction : BaseEntity<Guid>
{
    public required string Title { get; set; }

    public required string Description { get; set; }

    public required Currency Currency { get; set; }

    public required decimal Amount { get; set; }

    public required DateTime Time { get; set; }

    public required User Owner { get; set; }
}