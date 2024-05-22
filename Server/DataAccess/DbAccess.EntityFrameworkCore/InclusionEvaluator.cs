using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.Model;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore;

internal class InclusionEvaluator
{
    public static IQueryable<TEntity> Evaluate<TKey, TEntity>(IQueryable<TEntity> queryable, ImmutableArray<Inclusion<TKey, TEntity>> inclusions,
        CancellationToken cancellationToken = default)
        where TEntity : BaseEntity<TKey>
        where TKey : IParsable<TKey>,
        IEquatable<TKey>
    {
        ReadOnlySpan<string> includeStrings = BuildIncludeStrings(inclusions.As<Inclusion>().AsSpan(), cancellationToken: cancellationToken);

        foreach (string includeString in includeStrings)
        {
            queryable = queryable.Include(includeString);
        }

        return queryable;
    }

    private static ReadOnlySpan<string> BuildIncludeStrings(ReadOnlySpan<Inclusion> inclusions, string existingIncludeString = "", CancellationToken cancellationToken = default)
    {
        List<string> includeStrings = [];

        if (inclusions.IsEmpty && !string.IsNullOrWhiteSpace(existingIncludeString))
        {
            includeStrings.Add(existingIncludeString);
        }
        else
        {
            string buildPart = string.IsNullOrWhiteSpace(existingIncludeString) ? "" : existingIncludeString + '.';

            foreach (Inclusion inclusion in inclusions)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ReadOnlySpan<string> childInclusionStrings = BuildIncludeStrings(inclusion.ChildInclusions.AsSpan(),
                    buildPart + inclusion.IncludePropertyName, cancellationToken);

                includeStrings.AddRange(childInclusionStrings);
            }
        }

        return CollectionsMarshal.AsSpan(includeStrings);
    }
}