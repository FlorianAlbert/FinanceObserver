namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;

public abstract class BaseEntity<TKey> : LifecycleTrackable where TKey : IParsable<TKey>, 
                 IEquatable<TKey>
{
    public required TKey Id { get; init; }
}

public abstract class LifecycleTrackable
{
    private DateTimeOffset? _createdDate;
    public DateTimeOffset CreatedDate
    {
        get => _createdDate ?? DateTimeOffset.MinValue;
        internal set => _createdDate = value;
    }
    
    private DateTimeOffset? _updatedDate;
    public DateTimeOffset UpdatedDate
    {
        get => _updatedDate ?? DateTimeOffset.MinValue;
        internal set => _updatedDate = value;
    }
}