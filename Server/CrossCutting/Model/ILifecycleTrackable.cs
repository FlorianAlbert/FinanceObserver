namespace FlorianAlbert.FinanceObserver.Server.CrossCutting.Model;

public interface ILifecycleTrackable
{
    DateTimeOffset CreatedDate { get; set; }

    DateTimeOffset UpdatedDate { get; set; }
}