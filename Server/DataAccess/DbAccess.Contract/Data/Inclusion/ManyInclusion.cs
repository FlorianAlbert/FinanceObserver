using System.Linq.Expressions;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Data.Inclusion;

public class ManyInclusion<TEntity, TKey, TProperty> : Inclusion<TEntity, TKey, ICollection<TProperty>, TProperty>
    where TEntity : BaseEntity<TKey>
    where TKey : IParsable<TKey>,
    IEquatable<TKey>
    where TProperty : BaseEntity<TKey>
{
    public ManyInclusion(Expression<Func<TEntity, ICollection<TProperty>>> inclusion) : base(inclusion)
    {
    }
}