using System.Linq.Expressions;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Data.Inclusion;

public class SingleInclusion<TEntity, TKey, TProperty> : Inclusion<TEntity, TKey, TProperty, TProperty>
    where TEntity : BaseEntity<TKey>
    where TKey : IParsable<TKey>,
    IEquatable<TKey>
    where TProperty : BaseEntity<TKey>
{
    public SingleInclusion(Expression<Func<TEntity, TProperty>> inclusion) : base(inclusion)
    {
    }
}