namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;

public interface IBaseEntity<TKey> : ILifecycleTrackable where TKey : IParsable<TKey>, 
                 IEquatable<TKey>
{
    TKey Id { get; set; }
}
