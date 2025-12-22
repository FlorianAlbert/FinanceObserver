namespace FlorianAlbert.FinanceObserver.Server.CrossCutting.Model;

public interface IBaseEntity<TKey> : ILifecycleTrackable where TKey : IParsable<TKey>, 
                 IEquatable<TKey>
{
    TKey Id { get; set; }
}
