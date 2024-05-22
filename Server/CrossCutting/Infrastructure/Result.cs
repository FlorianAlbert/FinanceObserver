using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace FlorianAlbert.FinanceObserver.Server.CrossCutting.Infrastructure;

public class Result
{
    protected Result(bool succeeded, IEnumerable<Error>? errors = null)
    {
        Succeeded = succeeded;
        Errors = errors?.ToImmutableHashSet() ?? [];
    }

    public virtual bool Succeeded { get; }

    public virtual bool Failed => !Succeeded;

    public IReadOnlySet<Error> Errors { get; }

    public static Result Success()
    {
        return new Result(true);
    }

    public static Result Failure(IEnumerable<Error> errors)
    {
        return new Result(false, errors);
    }

    public static Result Failure(params Error[] errors)
    {
        return new Result(false, errors);
    }

    public static Result<T> Success<T>(T value)
    {
        return new Result<T>(true, value);
    }

    public static Result<T> Failure<T>(params Error[] errors)
    {
        return new Result<T>(false, default, errors);
    }

    public static Result<T> Failure<T>(IEnumerable<Error> errors)
    {
        return new Result<T>(false, default, errors);
    }
}

public class Result<T> : Result
{
    internal Result(bool succeeded, T? value = default, IEnumerable<Error>? errors = null) : base(succeeded, errors)
    {
        if (!succeeded && value is not null)
        {
            throw new InvalidResultException();
        }

        Value = value;
    }

    [MemberNotNullWhen(true, nameof(Value))]
    public override bool Succeeded => base.Succeeded;

    [MemberNotNullWhen(false, nameof(Value))]
    public override bool Failed => base.Failed;

    public T? Value { get; }
}