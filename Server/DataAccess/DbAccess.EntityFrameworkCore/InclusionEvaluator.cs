using System.Text;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Data;
using FlorianAlbert.FinanceObserver.Server.CrossCutting.Model;
using Microsoft.EntityFrameworkCore;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore;

internal class InclusionEvaluator
{
    private readonly StringBuilder _includeStringBuilder = new();

    public virtual IQueryable<TEntity> Evaluate<TEntity, TKey>(IQueryable<TEntity> queryable, Inclusion<TKey, TEntity>[] inclusions,
        CancellationToken cancellationToken = default)
        where TEntity : class, IBaseEntity<TKey>
        where TKey : IParsable<TKey>,
        IEquatable<TKey>
    {
        _includeStringBuilder.Clear();

        foreach (Inclusion<TKey, TEntity> inclusion in inclusions)
        {
            bool mustContinue;

            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                mustContinue = BuildIncludeString(inclusion, cancellationToken);

                queryable = queryable.Include(inclusion.IncludePropertyName + _includeStringBuilder);

                _includeStringBuilder.Clear();
            } while (mustContinue);
        }

        return queryable;
    }

    private bool BuildIncludeString(Inclusion inclusion, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (inclusion.ChildInclusions.Count == 0)
        {
            return false;
        }

        Inclusion nextInclusion = inclusion.ChildInclusions.Peek();

        _includeStringBuilder.Append('.').Append(nextInclusion.IncludePropertyName);

        bool mustContinue = BuildIncludeString(nextInclusion, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        if (mustContinue)
        {
            return true;
        }

        inclusion.ChildInclusions.Dequeue();

        return inclusion.ChildInclusions.Count > 0;
    }
}