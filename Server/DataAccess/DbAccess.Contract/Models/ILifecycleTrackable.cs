namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;

public interface ILifecycleTrackable
{
    DateTimeOffset CreatedDate { get; set; }

    DateTimeOffset UpdatedDate { get; set; }
}