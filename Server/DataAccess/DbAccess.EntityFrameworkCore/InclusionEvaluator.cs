using System.Text;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Data.Inclusion;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;
using Microsoft.EntityFrameworkCore;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore;

public class InclusionEvaluator : IInclusionEvaluator
{
    private readonly StringBuilder _includeStringBuilder = new();

    public IQueryable<T> Evaluate<T, TKey>(IQueryable<T> queryable, Inclusion[] inclusions,
        CancellationToken cancellationToken = default)
        where T : BaseEntity<TKey>
        where TKey : IParsable<TKey>,
        IEquatable<TKey>
    {
        _includeStringBuilder.Clear();

        foreach (var inclusion in inclusions)
        {
            var mustContinue = false;

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

        var nextInclusion = inclusion.ChildInclusions.Peek();

        _includeStringBuilder.Append('.').Append(nextInclusion.IncludePropertyName);

        var mustContinue = BuildIncludeString(nextInclusion, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        if (mustContinue)
        {
            return true;
        }

        inclusion.ChildInclusions.Dequeue();

        return inclusion.ChildInclusions.Count > 0;
    }
}