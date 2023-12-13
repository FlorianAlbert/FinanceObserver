namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;

public abstract class BaseEntity<TKey>
    where TKey : IParsable<TKey>, 
                 IEquatable<TKey>
{
    public required TKey Id { get; init; }

    private DateTime? _createdDate;
    public DateTime CreatedDate
    {
        get => _createdDate ?? DateTime.MinValue;
        internal set => _createdDate = value;
    }
    
    private DateTime? _updatedDate;
    public DateTime UpdatedDate
    {
        get => _updatedDate ?? DateTime.MinValue;
        internal set => _updatedDate = value;
    }
}