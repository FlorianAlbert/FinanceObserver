using System.Linq.Expressions;
using System.Reflection;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Data;

public abstract class Inclusion
{
    protected readonly Queue<Inclusion> _childInclusions = new();

    internal abstract string IncludePropertyName { get; private protected set; }
    internal Queue<Inclusion> ChildInclusions => _childInclusions;
    
    private bool Equals(Inclusion other)
    {
        return Equals(_childInclusions, other._childInclusions) && IncludePropertyName == other.IncludePropertyName;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return Equals((Inclusion)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_childInclusions, IncludePropertyName);
    }

    public static bool operator ==(Inclusion? left, Inclusion? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Inclusion? left, Inclusion? right)
    {
        return !Equals(left, right);
    }
}

public abstract class Inclusion<TKey, TEntity> : Inclusion
    where TEntity : BaseEntity<TKey>?
    where TKey : IParsable<TKey>,
    IEquatable<TKey>
{
    public static Inclusion<TEntity, TKey, ICollection<TProperty>, TProperty> Of<TProperty>(Expression<Func<TEntity, ICollection<TProperty>>> inclusion)
        where TProperty : BaseEntity<TKey>
    {
        return new Inclusion<TEntity, TKey, ICollection<TProperty>, TProperty>(inclusion);
    }

    public static Inclusion<TEntity, TKey, TProperty, TProperty> Of<TProperty>(Expression<Func<TEntity, TProperty>> inclusion)
        where TProperty : BaseEntity<TKey>?
    {
        return new Inclusion<TEntity, TKey, TProperty, TProperty>(inclusion);
    }
}

public class Inclusion<TEntity, TKey, TInclude, TProperty> : Inclusion<TKey, TEntity>
    where TEntity : BaseEntity<TKey>?
    where TKey : IParsable<TKey>,
    IEquatable<TKey>
    where TProperty : BaseEntity<TKey>?
{
    internal Inclusion(Expression<Func<TEntity, TInclude>> inclusion)
    {
        if (inclusion.Body is not MemberExpression member)
        {
            throw new ArgumentException($"Expression '{inclusion}' refers to a method, not a property.");
        }

        if (member.Expression is not ParameterExpression)
        {
            throw new ArgumentException($"Expression '{inclusion}' refers to a method, not a property.");
        }

        if (member.Member is not PropertyInfo propInfo)
        {
            throw new ArgumentException($"Expression '{inclusion}' refers to a field, not a property.");
        }

        var type = typeof(TEntity);
        if (propInfo.ReflectedType != null && !propInfo.ReflectedType.IsAssignableFrom(type))
        {
            throw new ArgumentException($"Expression '{inclusion}' refers to a property that is not from type {type}.");
        }

        IncludePropertyName = propInfo.Name;
    }

    internal sealed override string IncludePropertyName { get; private protected set; }

    public Inclusion<TEntity, TKey, TInclude, TProperty> AddChildInclusion<TNestedProperty>(
        Inclusion<TProperty, TKey, TNestedProperty, TNestedProperty> childInclusion)
        where TNestedProperty : BaseEntity<TKey>
    {
        _childInclusions.Enqueue(childInclusion);

        return this;
    }

    public Inclusion<TEntity, TKey, TInclude, TProperty> AddChildInclusion<TCollection, TContent>(
        Inclusion<TProperty, TKey, TCollection, TContent> childInclusion)
        where TCollection : class, ICollection<TContent>
        where TContent : BaseEntity<TKey>
    {
        _childInclusions.Enqueue(childInclusion);

        return this;
    }
}