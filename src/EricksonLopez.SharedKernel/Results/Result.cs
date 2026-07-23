using System.Diagnostics.CodeAnalysis;

namespace EricksonLopez.SharedKernel.Results;

/// <summary>
/// Represents the outcome of an operation that may succeed or fail.
/// </summary>
/// <remarks>
/// <para>
/// Use Result to make failure an explicit part of your method signatures.
/// Never throw exceptions for expected domain failures — use Result instead.
/// </para>
/// <para>
/// <b>Usage:</b>
/// <code>
/// // Returning success or failure
/// return Result.Success();
/// return Result.Failure(UserErrors.NotFound(id));
///
/// // Pattern matching with Match
/// return result.Match(
///     () =&gt; NoContent(),
///     error =&gt; Problem(error.Description));
///
/// // Wrapping exceptions
/// var result = Result.Try(() =&gt; riskyOperation(), ex =&gt; Error.Unexpected("Op.Failed", ex.Message));
/// </code>
/// </para>
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

    private static readonly Result _success = new(true, Error.None);

    public static Result Success() => _success;
    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => Result<TValue>.Success(value);
    public static Result<TValue> Failure<TValue>(Error error) => Result<TValue>.Failure(error);

    // ─── Match ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Forces handling of both success and failure cases.
    /// </summary>
    /// <example>
    /// <code>
    /// return result.Match(
    ///     () =&gt; NoContent(),
    ///     error =&gt; Problem(error.Description));
    /// </code>
    /// </example>
    public TOut Match<TOut>(Func<TOut> onSuccess, Func<Error, TOut> onFailure)
        => IsSuccess ? onSuccess() : onFailure(Error);

    // ─── Side effects ─────────────────────────────────────────────────────────

    /// <summary>
    /// Executes <paramref name="onSuccess"/> if the result is a success.
    /// Returns the result unchanged for chaining.
    /// </summary>
    public Result Tap(Action onSuccess)
    {
        if (IsSuccess) onSuccess();
        return this;
    }

    /// <summary>
    /// Executes <paramref name="onFailure"/> if the result is a failure.
    /// Returns the result unchanged for chaining.
    /// </summary>
    public Result TapError(Action<Error> onFailure)
    {
        if (IsFailure) onFailure(Error);
        return this;
    }

    // ─── Composition ──────────────────────────────────────────────────────────

    /// <summary>
    /// Validates a condition after success. If the predicate returns false,
    /// the result becomes a failure with the specified error.
    /// </summary>
    public Result Ensure(Func<bool> predicate, Error error)
    {
        if (IsFailure) return this;
        return predicate() ? this : Failure(error);
    }

    // ─── Termination ──────────────────────────────────────────────────────────

    /// <summary>
    /// Executes <paramref name="action"/> regardless of success or failure state.
    /// Returns the result unchanged.
    /// </summary>
    public Result Finally(Action<Result> action)
    {
        action(this);
        return this;
    }

    // ─── Exception bridge ─────────────────────────────────────────────────────

    /// <summary>
    /// Wraps a void action that might throw, converting exceptions to errors.
    /// </summary>
    /// <example>
    /// <code>
    /// var result = Result.Try(
    ///     () =&gt; File.Delete(path),
    ///     ex =&gt; Error.Unexpected("File.DeleteFailed", ex.Message));
    /// </code>
    /// </example>
    public static Result Try(Action action, Func<Exception, Error> errorHandler)
    {
        try
        {
            action();
            return Success();
        }
        catch (Exception ex)
        {
            return Failure(errorHandler(ex));
        }
    }

    /// <summary>
    /// Wraps a function that might throw, converting exceptions to errors.
    /// </summary>
    /// <example>
    /// <code>
    /// var result = Result.Try(
    ///     () =&gt; int.Parse(input),
    ///     ex =&gt; Error.Validation("Parse.Failed", ex.Message));
    /// </code>
    /// </example>
    public static Result<T> Try<T>(Func<T> func, Func<Exception, Error> errorHandler)
    {
        try
        {
            return Success(func());
        }
        catch (Exception ex)
        {
            return Failure<T>(errorHandler(ex));
        }
    }

    // ─── Combine ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Aggregates multiple results. Returns success if all succeed,
    /// or a compound failure containing all errors.
    /// </summary>
    public static Result Combine(params Result[] results)
    {
        List<Error>? errors = null;

        foreach (var result in results)
        {
            if (result.IsFailure)
            {
                errors ??= [];
                errors.Add(result.Error);
            }
        }

        if (errors is null)
            return Success();

        return errors.Count == 1
            ? Failure(errors[0])
            : Failure(Error.Failure(
                "Result.CombinedErrors",
                $"{errors.Count} errors occurred",
                [.. errors]));
    }

    /// <summary>
    /// Aggregates homogeneous typed results into a list.
    /// Returns all values on success, or a compound failure on any error.
    /// </summary>
    public static Result<IReadOnlyList<T>> Combine<T>(params Result<T>[] results)
    {
        List<Error>? errors = null;
        var values = new List<T>(results.Length);

        foreach (var result in results)
        {
            if (result.IsFailure)
            {
                errors ??= [];
                errors.Add(result.Error);
            }
            else
            {
                values.Add(result.Value);
            }
        }

        if (errors is null)
            return Success<IReadOnlyList<T>>(values.AsReadOnly());

        return errors.Count == 1
            ? Failure<IReadOnlyList<T>>(errors[0])
            : Failure<IReadOnlyList<T>>(Error.Failure(
                "Result.CombinedErrors",
                $"{errors.Count} errors occurred",
                [.. errors]));
    }

    /// <summary>
    /// Aggregates two typed results into a value tuple.
    /// </summary>
    public static Result<(T1, T2)> Combine<T1, T2>(Result<T1> r1, Result<T2> r2)
    {
        if (r1.IsSuccess && r2.IsSuccess)
            return Success((r1.Value, r2.Value));

        List<Error> errors = [];
        if (r1.IsFailure) errors.Add(r1.Error);
        if (r2.IsFailure) errors.Add(r2.Error);

        return errors.Count == 1
            ? Failure<(T1, T2)>(errors[0])
            : Failure<(T1, T2)>(Error.Failure(
                "Result.CombinedErrors",
                $"{errors.Count} errors occurred",
                [.. errors]));
    }

    /// <summary>
    /// Aggregates three typed results into a value tuple.
    /// </summary>
    public static Result<(T1, T2, T3)> Combine<T1, T2, T3>(
        Result<T1> r1, Result<T2> r2, Result<T3> r3)
    {
        if (r1.IsSuccess && r2.IsSuccess && r3.IsSuccess)
            return Success((r1.Value, r2.Value, r3.Value));

        List<Error> errors = [];
        if (r1.IsFailure) errors.Add(r1.Error);
        if (r2.IsFailure) errors.Add(r2.Error);
        if (r3.IsFailure) errors.Add(r3.Error);

        return errors.Count == 1
            ? Failure<(T1, T2, T3)>(errors[0])
            : Failure<(T1, T2, T3)>(Error.Failure(
                "Result.CombinedErrors",
                $"{errors.Count} errors occurred",
                [.. errors]));
    }

    // ─── Try-pattern ──────────────────────────────────────────────────────────

    /// <summary>
    /// Tries to extract the error. Returns <c>true</c> if the result is a failure.
    /// </summary>
    /// <example>
    /// <code>
    /// if (result.TryGetError(out var error))
    ///     _logger.LogWarning("{Error}", error);
    /// </code>
    /// </example>
    public bool TryGetError([MaybeNullWhen(false)] out Error error)
    {
        error = IsFailure ? Error : default;
        return IsFailure;
    }

    // ─── Error transformation ─────────────────────────────────────────────────

    /// <summary>
    /// Transforms the error while preserving the success/failure state.
    /// If the result is a success, returns it unchanged.
    /// </summary>
    /// <example>
    /// <code>
    /// var adapted = result.MapError(e =&gt;
    ///     Error.Failure("App.Error", $"Operation failed: {e.Description}"));
    /// </code>
    /// </example>
    public Result MapError(Func<Error, Error> mapper)
        => IsFailure ? Failure(mapper(Error)) : this;

    // ─── Implicit conversions ─────────────────────────────────────────────────

    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>
/// Represents the outcome of an operation that produces a value on success.
/// </summary>
/// <remarks>
/// <para>
/// Supports monadic composition via <see cref="Map{TNext}"/> and <see cref="Bind{TNext}"/>,
/// exhaustive handling via <see cref="Match{TOut}"/>, and fluent side effects via
/// <see cref="Tap"/> and <see cref="TapError"/>.
/// </para>
/// <para>
/// <b>Pipeline example:</b>
/// <code>
/// var result = GetUser(id)
///     .Ensure(u =&gt; u.IsActive, UserErrors.Inactive)
///     .Map(u =&gt; u.ToDto())
///     .Tap(dto =&gt; _cache.Set(id, dto))
///     .TapError(e =&gt; _logger.LogWarning("Failed: {Error}", e));
/// </code>
/// </para>
/// </remarks>
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

    // ─── Match ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Forces handling of both success and failure cases.
    /// </summary>
    /// <example>
    /// <code>
    /// return result.Match(
    ///     user =&gt; Ok(user),
    ///     error =&gt; Problem(error.Description));
    /// </code>
    /// </example>
    public TOut Match<TOut>(Func<TValue, TOut> onSuccess, Func<Error, TOut> onFailure)
        => IsSuccess ? onSuccess(Value) : onFailure(Error);

    // ─── Side effects ─────────────────────────────────────────────────────────

    /// <summary>
    /// Executes <paramref name="action"/> with the value if successful.
    /// Returns the result unchanged for chaining.
    /// </summary>
    public Result<TValue> Tap(Action<TValue> action)
    {
        if (IsSuccess) action(Value);
        return this;
    }

    /// <summary>
    /// Executes <paramref name="action"/> with the error if failed.
    /// Returns the result unchanged for chaining.
    /// </summary>
    public new Result<TValue> TapError(Action<Error> action)
    {
        if (IsFailure) action(Error);
        return this;
    }

    // ─── Composition ──────────────────────────────────────────────────────────

    /// <summary>
    /// Validates a condition on the success value. If the predicate returns false,
    /// the result becomes a failure with the specified error.
    /// </summary>
    /// <example>
    /// <code>
    /// var result = GetUser(id)
    ///     .Ensure(u =&gt; u.IsActive, Error.Forbidden("User.Inactive", "User is not active"));
    /// </code>
    /// </example>
    public Result<TValue> Ensure(Func<TValue, bool> predicate, Error error)
    {
        if (IsFailure) return this;
        return predicate(Value) ? this : Result.Failure<TValue>(error);
    }

    // ─── Recovery ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Attempts to recover from a failure by applying a recovery function.
    /// If the result is already successful, returns it unchanged.
    /// </summary>
    /// <example>
    /// <code>
    /// var result = GetFromPrimary(id)
    ///     .Recover(error =&gt; GetFromCache(id));
    /// </code>
    /// </example>
    public Result<TValue> Recover(Func<Error, Result<TValue>> recovery)
        => IsFailure ? recovery(Error) : this;

    // ─── Termination ──────────────────────────────────────────────────────────

    /// <summary>
    /// Executes <paramref name="action"/> regardless of success or failure state.
    /// Returns the result unchanged.
    /// </summary>
    public Result<TValue> Finally(Action<Result<TValue>> action)
    {
        action(this);
        return this;
    }

    // ─── Try-pattern ──────────────────────────────────────────────────────────

    /// <summary>
    /// Tries to extract the success value. Returns <c>true</c> if successful.
    /// </summary>
    /// <example>
    /// <code>
    /// if (result.TryGetValue(out var user))
    ///     Console.WriteLine(user.Name);
    /// </code>
    /// </example>
    public bool TryGetValue([MaybeNullWhen(false)] out TValue value)
    {
        value = IsSuccess ? _value! : default;
        return IsSuccess;
    }

    /// <summary>
    /// Tries to extract the error. Returns <c>true</c> if the result is a failure.
    /// </summary>
    public new bool TryGetError([MaybeNullWhen(false)] out Error error)
    {
        error = IsFailure ? Error : default;
        return IsFailure;
    }

    // ─── Safe access ──────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the value if success, or <paramref name="defaultValue"/> if failure.
    /// </summary>
    /// <example>
    /// <code>
    /// var name = GetUser(id)
    ///     .Map(u =&gt; u.Name)
    ///     .GetValueOrDefault("Unknown");
    /// </code>
    /// </example>
    public TValue GetValueOrDefault(TValue defaultValue)
        => IsSuccess ? _value! : defaultValue;

    /// <summary>
    /// Returns the value if success, or invokes <paramref name="fallback"/> with the error.
    /// </summary>
    /// <example>
    /// <code>
    /// var config = LoadConfig()
    ///     .GetValueOrDefault(error =&gt; Config.Default);
    /// </code>
    /// </example>
    public TValue GetValueOrDefault(Func<Error, TValue> fallback)
        => IsSuccess ? _value! : fallback(Error);

    // ─── Error transformation ─────────────────────────────────────────────────

    /// <summary>
    /// Transforms the error while preserving the value type.
    /// If the result is a success, returns it unchanged.
    /// </summary>
    /// <example>
    /// <code>
    /// var adapted = repositoryResult.MapError(e =&gt;
    ///     Error.Failure("App.Error", $"Operation failed: {e.Description}"));
    /// </code>
    /// </example>
    public new Result<TValue> MapError(Func<Error, Error> mapper)
        => IsFailure ? Result.Failure<TValue>(mapper(Error)) : this;

    // ─── Conversion ───────────────────────────────────────────────────────────

    /// <summary>
    /// Drops the value, converting <see cref="Result{TValue}"/> to <see cref="Result"/>.
    /// Preserves success/failure state and error.
    /// </summary>
    /// <example>
    /// <code>
    /// // Command handler that doesn't return a value
    /// public Task&lt;Result&gt; Handle(CreateOrderCommand cmd)
    ///     =&gt; _repo.Save(entity).Map(_ =&gt; /* ... */).ToResult();
    /// </code>
    /// </example>
    public Result ToResult()
        => IsSuccess ? Result.Success() : Result.Failure(Error);

    // ─── Deconstruct ──────────────────────────────────────────────────────────

    /// <summary>
    /// Enables destructuring: <c>var (isSuccess, value, error) = result;</c>
    /// </summary>
    /// <example>
    /// <code>
    /// var (ok, user, error) = GetUser(id);
    /// if (ok) Console.WriteLine(user.Name);
    /// </code>
    /// </example>
    public void Deconstruct(out bool isSuccess, out TValue? value, out Error error)
    {
        isSuccess = IsSuccess;
        value = IsSuccess ? _value : default;
        error = Error;
    }

    // ─── Implicit conversions ─────────────────────────────────────────────────

    public static implicit operator Result<TValue>(TValue value) => Success(value);
    public static implicit operator Result<TValue>(Error error) => Failure(error);
}
