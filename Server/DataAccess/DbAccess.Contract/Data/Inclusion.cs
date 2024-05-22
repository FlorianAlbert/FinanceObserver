using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.Model;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Data;

public abstract class Inclusion
{
    protected readonly List<Inclusion> _childInclusions = [];

    public abstract string IncludePropertyName { get; private protected set; }

    public ImmutableArray<Inclusion> ChildInclusions => [.. _childInclusions];

    private bool Equals(Inclusion other)
    {
        return Equals(_childInclusions, other._childInclusions) && IncludePropertyName == other.IncludePropertyName;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
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

        return Equals((Inclusion) obj);
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
    where TKey : IParsable<TKey>, IEquatable<TKey>
{
    public static Inclusion<TKey, TEntity, TPropertyKey, TProperty, ICollection<TProperty>> Of<TPropertyKey, TProperty>(Expression<Func<TEntity, ICollection<TProperty>>> inclusion)
        where TProperty : BaseEntity<TPropertyKey>
        where TPropertyKey : IParsable<TPropertyKey>,
        IEquatable<TPropertyKey>
    {
        return new Inclusion<TKey, TEntity, TPropertyKey, TProperty, ICollection<TProperty>>(inclusion);
    }

    public static Inclusion<TKey, TEntity, TPropertyKey, TProperty, TProperty> Of<TPropertyKey, TProperty>(Expression<Func<TEntity, TProperty>> inclusion)
        where TProperty : BaseEntity<TPropertyKey>?
        where TPropertyKey : IParsable<TPropertyKey>,
        IEquatable<TPropertyKey>
    {
        return new Inclusion<TKey, TEntity, TPropertyKey, TProperty, TProperty>(inclusion);
    }
}

public class Inclusion<TKey, TEntity, TPropertyKey, TProperty, TInclude> : Inclusion<TKey, TEntity>
    where TEntity : BaseEntity<TKey>?
    where TKey : IParsable<TKey>,
    IEquatable<TKey>
    where TProperty : BaseEntity<TPropertyKey>?
    where TPropertyKey : IParsable<TPropertyKey>,
    IEquatable<TPropertyKey>
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

        Type type = typeof(TEntity);
        if (propInfo.ReflectedType != null && !propInfo.ReflectedType.IsAssignableFrom(type))
        {
            throw new ArgumentException($"Expression '{inclusion}' refers to a property that is not from type {type}.");
        }

        IncludePropertyName = propInfo.Name;
    }

    public sealed override string IncludePropertyName { get; private protected set; }

    public Inclusion<TKey, TEntity, TPropertyKey, TProperty, TInclude> AddChildInclusion<TNestedPropertyKey, TNestedProperty>(
        Inclusion<TPropertyKey, TProperty, TNestedPropertyKey, TNestedProperty, TNestedProperty> childInclusion)
        where TNestedProperty : BaseEntity<TNestedPropertyKey>
        where TNestedPropertyKey : IParsable<TNestedPropertyKey>,
        IEquatable<TNestedPropertyKey>
    {
        _childInclusions.Add(childInclusion);

        return this;
    }

    public Inclusion<TKey, TEntity, TPropertyKey, TProperty, TInclude> AddChildInclusion<TContentKey, TContent, TCollection>(
        Inclusion<TPropertyKey, TProperty, TContentKey, TContent, TCollection> childInclusion)
        where TCollection : class, ICollection<TContent>
        where TContent : BaseEntity<TContentKey>
        where TContentKey : IParsable<TContentKey>,
        IEquatable<TContentKey>
    {
        _childInclusions.Add(childInclusion);

        return this;
    }
}