using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.Exceptions;

namespace FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.InfrastructureTypes;

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
}

public class Result<T> : Result
{
    private Result(bool succeeded, T? value = default, IEnumerable<Error>? errors = null) : base(succeeded, errors)
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

    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value);
    }

    public new static Result<T> Failure(params Error[] errors)
    {
        return new Result<T>(false, default, errors);
    }

    public new static Result<T> Failure(IEnumerable<Error> errors)
    {
        return new Result<T>(false, default, errors);
    }
}