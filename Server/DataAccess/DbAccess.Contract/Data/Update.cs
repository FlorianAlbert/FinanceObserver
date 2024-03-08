using System.Linq.Expressions;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Data;

public class Update<TEntity>
{
    internal LambdaExpression SelectorExpression { get; }

    internal LambdaExpression ValueExpression { get; }

    private Update(LambdaExpression selectorExpression, LambdaExpression valueExpression)
    {
        SelectorExpression = selectorExpression;
        ValueExpression = valueExpression;
    }

    public static Update<TEntity> With<TProperty>(Expression<Func<TEntity, TProperty>> selectorExpression,
        Expression<Func<TEntity, TProperty>> valueExpression)
    {
        return new Update<TEntity>(selectorExpression, valueExpression);
    }

    public static Update<TEntity> With<TProperty>(Expression<Func<TEntity, TProperty>> selectorExpression, TProperty value)
    {
        return new Update<TEntity>(selectorExpression, (TEntity e) => value);
    }

    public bool SelectorEquals<TProperty>(Expression<Func<TEntity, TProperty>> otherSelectorExpression)
    {
        if (SelectorExpression is not Expression<Func<TEntity, TProperty>> thisSelectorExpression)
        {
            return false;
        }

        return otherSelectorExpression.Body == thisSelectorExpression.Body;
    }
}