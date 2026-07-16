namespace EricksonLopez.SharedKernel.Results;

/// <summary>
/// Represents the outcome of an operation that may succeed or fail.
/// </summary>
/// <remarks>
/// Use Result to make failure an explicit part of your method signatures.
/// Never throw exceptions for expected domain failures — use Result instead.
///
/// Usage:
/// <code>
/// // Returning success
/// return Result.Success();
///
/// // Returning failure
/// return Result.Failure(UserErrors.NotFound(id));
///
/// // Matching
/// if (result.IsFailure)
///     return result.Error;
/// </code>
/// </remarks>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("A successful result cannot have an error.");

        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("A failure result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    // ─── Factory methods ──────────────────────────────────────────────────────

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => Result<TValue>.Success(value);
    public static Result<TValue> Failure<TValue>(Error error) => Result<TValue>.Failure(error);

    // ─── Implicit conversions ─────────────────────────────────────────────────

    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>
/// Represents the outcome of an operation that produces a value on success.
/// </summary>
/// <typeparam name="TValue">The type of the success value.</typeparam>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    private Result(TValue value) : base(true, Error.None)
        => _value = value;

    private Result(Error error) : base(false, error)
        => _value = default;

    /// <summary>
    /// The success value. Throws if accessed on a failed result.
    /// </summary>
    /// <exception cref="InvalidOperationException">When accessed on a failure result.</exception>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException(
            $"Cannot access Value of a failed result. Error: {Error}");

    // ─── Factory methods ──────────────────────────────────────────────────────

    public static Result<TValue> Success(TValue value) => new(value);
    public new static Result<TValue> Failure(Error error) => new(error);

    // ─── Monadic operations ───────────────────────────────────────────────────

    /// <summary>
    /// Maps the success value to a new type.
    /// If the result is a failure, the error is propagated.
    /// </summary>
    public Result<TNext> Map<TNext>(Func<TValue, TNext> mapper)
        => IsSuccess ? Result.Success(mapper(Value)) : Result.Failure<TNext>(Error);

    /// <summary>
    /// Chains a function that also returns a Result.
    /// If the result is a failure, the error is propagated without invoking <paramref name="bind"/>.
    /// </summary>
    public Result<TNext> Bind<TNext>(Func<TValue, Result<TNext>> bind)
        => IsSuccess ? bind(Value) : Result.Failure<TNext>(Error);

    // ─── Implicit conversions ─────────────────────────────────────────────────

    public static implicit operator Result<TValue>(TValue value) => Success(value);
    public static implicit operator Result<TValue>(Error error) => Failure(error);
}
