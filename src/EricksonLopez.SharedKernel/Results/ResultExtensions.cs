namespace EricksonLopez.SharedKernel.Results;

/// <summary>
/// Async extension methods for composing <see cref="Result"/> and <see cref="Result{T}"/>
/// pipelines over <see cref="Task"/> and <see cref="ValueTask"/>.
/// </summary>
/// <remarks>
/// All methods use <c>ConfigureAwait(false)</c> internally — libraries should
/// not capture the synchronization context.
/// <para>
/// <b>Pipeline example:</b>
/// <code>
/// var result = await _repository.GetById(id)    // Task&lt;Result&lt;User&gt;&gt;
///     .Ensure(u =&gt; u.IsActive, UserErrors.Inactive)
///     .Map(u =&gt; u.ToDto())
///     .Tap(dto =&gt; _cache.Set(id, dto))
///     .TapError(e =&gt; _logger.LogWarning("{Error}", e));
/// </code>
/// </para>
/// </remarks>
public static class ResultExtensions
{
    // ══════════════════════════════════════════════════════════════════════════
    //  Task<Result<T>> extensions
    // ══════════════════════════════════════════════════════════════════════════

    // ── Map ───────────────────────────────────────────────────────────────────

    /// <summary>Maps the success value using a synchronous mapper.</summary>
    public static async Task<Result<TNext>> Map<T, TNext>(
        this Task<Result<T>> resultTask, Func<T, TNext> mapper)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Map(mapper);
    }

    /// <summary>Maps the success value using an async mapper.</summary>
    public static async Task<Result<TNext>> Map<T, TNext>(
        this Task<Result<T>> resultTask, Func<T, Task<TNext>> mapper)
    {
        var result = await resultTask.ConfigureAwait(false);
        if (result.IsFailure) return Result.Failure<TNext>(result.Error);
        var next = await mapper(result.Value).ConfigureAwait(false);
        return Result.Success(next);
    }

    // ── Bind ──────────────────────────────────────────────────────────────────

    /// <summary>Chains a synchronous function that returns a Result.</summary>
    public static async Task<Result<TNext>> Bind<T, TNext>(
        this Task<Result<T>> resultTask, Func<T, Result<TNext>> bind)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Bind(bind);
    }

    /// <summary>Chains an async function that returns a Result.</summary>
    public static async Task<Result<TNext>> Bind<T, TNext>(
        this Task<Result<T>> resultTask, Func<T, Task<Result<TNext>>> bind)
    {
        var result = await resultTask.ConfigureAwait(false);
        if (result.IsFailure) return Result.Failure<TNext>(result.Error);
        return await bind(result.Value).ConfigureAwait(false);
    }

    // ── Match ─────────────────────────────────────────────────────────────────

    /// <summary>Forces handling of both success and failure cases.</summary>
    public static async Task<TOut> Match<T, TOut>(
        this Task<Result<T>> resultTask,
        Func<T, TOut> onSuccess, Func<Error, TOut> onFailure)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Match(onSuccess, onFailure);
    }

    // ── Tap ───────────────────────────────────────────────────────────────────

    /// <summary>Executes a synchronous side effect on success.</summary>
    public static async Task<Result<T>> Tap<T>(
        this Task<Result<T>> resultTask, Action<T> action)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Tap(action);
    }

    /// <summary>Executes an async side effect on success.</summary>
    public static async Task<Result<T>> Tap<T>(
        this Task<Result<T>> resultTask, Func<T, Task> action)
    {
        var result = await resultTask.ConfigureAwait(false);
        if (result.IsSuccess) await action(result.Value).ConfigureAwait(false);
        return result;
    }

    // ── TapError ──────────────────────────────────────────────────────────────

    /// <summary>Executes a side effect on failure.</summary>
    public static async Task<Result<T>> TapError<T>(
        this Task<Result<T>> resultTask, Action<Error> action)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.TapError(action);
    }

    // ── Ensure ────────────────────────────────────────────────────────────────

    /// <summary>Validates a condition on the success value.</summary>
    public static async Task<Result<T>> Ensure<T>(
        this Task<Result<T>> resultTask, Func<T, bool> predicate, Error error)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Ensure(predicate, error);
    }

    /// <summary>Validates a condition using an async predicate.</summary>
    public static async Task<Result<T>> Ensure<T>(
        this Task<Result<T>> resultTask, Func<T, Task<bool>> predicate, Error error)
    {
        var result = await resultTask.ConfigureAwait(false);
        if (result.IsFailure) return result;
        return await predicate(result.Value).ConfigureAwait(false)
            ? result
            : Result.Failure<T>(error);
    }

    // ── Recover ───────────────────────────────────────────────────────────────

    /// <summary>Attempts synchronous recovery from failure.</summary>
    public static async Task<Result<T>> Recover<T>(
        this Task<Result<T>> resultTask, Func<Error, Result<T>> recovery)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Recover(recovery);
    }

    /// <summary>Attempts async recovery from failure.</summary>
    public static async Task<Result<T>> Recover<T>(
        this Task<Result<T>> resultTask, Func<Error, Task<Result<T>>> recovery)
    {
        var result = await resultTask.ConfigureAwait(false);
        if (result.IsSuccess) return result;
        return await recovery(result.Error).ConfigureAwait(false);
    }

    // ── MapError ──────────────────────────────────────────────────────────────

    /// <summary>Transforms the error while preserving the value type.</summary>
    public static async Task<Result<T>> MapError<T>(
        this Task<Result<T>> resultTask, Func<Error, Error> mapper)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.MapError(mapper);
    }

    // ── Finally ───────────────────────────────────────────────────────────────

    /// <summary>Executes action regardless of success or failure.</summary>
    public static async Task<Result<T>> Finally<T>(
        this Task<Result<T>> resultTask, Action<Result<T>> action)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Finally(action);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Task<Result> (non-generic) extensions
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Forces handling of both success and failure cases.</summary>
    public static async Task<TOut> Match<TOut>(
        this Task<Result> resultTask,
        Func<TOut> onSuccess, Func<Error, TOut> onFailure)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Match(onSuccess, onFailure);
    }

    /// <summary>Executes a side effect on success.</summary>
    public static async Task<Result> Tap(
        this Task<Result> resultTask, Action onSuccess)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Tap(onSuccess);
    }

    /// <summary>Executes a side effect on failure.</summary>
    public static async Task<Result> TapError(
        this Task<Result> resultTask, Action<Error> onFailure)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.TapError(onFailure);
    }

    /// <summary>Validates a condition after success.</summary>
    public static async Task<Result> Ensure(
        this Task<Result> resultTask, Func<bool> predicate, Error error)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Ensure(predicate, error);
    }

    /// <summary>Transforms the error while preserving the success/failure state.</summary>
    public static async Task<Result> MapError(
        this Task<Result> resultTask, Func<Error, Error> mapper)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.MapError(mapper);
    }

    /// <summary>Executes action regardless of success or failure.</summary>
    public static async Task<Result> Finally(
        this Task<Result> resultTask, Action<Result> action)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Finally(action);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  ValueTask<Result<T>> extensions
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Maps the success value using a synchronous mapper.</summary>
    public static async ValueTask<Result<TNext>> Map<T, TNext>(
        this ValueTask<Result<T>> resultTask, Func<T, TNext> mapper)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Map(mapper);
    }

    /// <summary>Maps the success value using an async mapper.</summary>
    public static async ValueTask<Result<TNext>> Map<T, TNext>(
        this ValueTask<Result<T>> resultTask, Func<T, ValueTask<TNext>> mapper)
    {
        var result = await resultTask.ConfigureAwait(false);
        if (result.IsFailure) return Result.Failure<TNext>(result.Error);
        var next = await mapper(result.Value).ConfigureAwait(false);
        return Result.Success(next);
    }

    /// <summary>Chains a synchronous function that returns a Result.</summary>
    public static async ValueTask<Result<TNext>> Bind<T, TNext>(
        this ValueTask<Result<T>> resultTask, Func<T, Result<TNext>> bind)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Bind(bind);
    }

    /// <summary>Chains an async function that returns a Result.</summary>
    public static async ValueTask<Result<TNext>> Bind<T, TNext>(
        this ValueTask<Result<T>> resultTask, Func<T, ValueTask<Result<TNext>>> bind)
    {
        var result = await resultTask.ConfigureAwait(false);
        if (result.IsFailure) return Result.Failure<TNext>(result.Error);
        return await bind(result.Value).ConfigureAwait(false);
    }

    /// <summary>Forces handling of both success and failure cases.</summary>
    public static async ValueTask<TOut> Match<T, TOut>(
        this ValueTask<Result<T>> resultTask,
        Func<T, TOut> onSuccess, Func<Error, TOut> onFailure)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Match(onSuccess, onFailure);
    }

    /// <summary>Executes a synchronous side effect on success.</summary>
    public static async ValueTask<Result<T>> Tap<T>(
        this ValueTask<Result<T>> resultTask, Action<T> action)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Tap(action);
    }

    /// <summary>Executes an async side effect on success.</summary>
    public static async ValueTask<Result<T>> Tap<T>(
        this ValueTask<Result<T>> resultTask, Func<T, ValueTask> action)
    {
        var result = await resultTask.ConfigureAwait(false);
        if (result.IsSuccess) await action(result.Value).ConfigureAwait(false);
        return result;
    }

    /// <summary>Executes a side effect on failure.</summary>
    public static async ValueTask<Result<T>> TapError<T>(
        this ValueTask<Result<T>> resultTask, Action<Error> action)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.TapError(action);
    }

    /// <summary>Validates a condition on the success value.</summary>
    public static async ValueTask<Result<T>> Ensure<T>(
        this ValueTask<Result<T>> resultTask, Func<T, bool> predicate, Error error)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Ensure(predicate, error);
    }

    /// <summary>Validates a condition using an async predicate.</summary>
    public static async ValueTask<Result<T>> Ensure<T>(
        this ValueTask<Result<T>> resultTask, Func<T, ValueTask<bool>> predicate, Error error)
    {
        var result = await resultTask.ConfigureAwait(false);
        if (result.IsFailure) return result;
        return await predicate(result.Value).ConfigureAwait(false)
            ? result
            : Result.Failure<T>(error);
    }

    /// <summary>Attempts synchronous recovery from failure.</summary>
    public static async ValueTask<Result<T>> Recover<T>(
        this ValueTask<Result<T>> resultTask, Func<Error, Result<T>> recovery)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Recover(recovery);
    }

    /// <summary>Attempts async recovery from failure.</summary>
    public static async ValueTask<Result<T>> Recover<T>(
        this ValueTask<Result<T>> resultTask, Func<Error, ValueTask<Result<T>>> recovery)
    {
        var result = await resultTask.ConfigureAwait(false);
        if (result.IsSuccess) return result;
        return await recovery(result.Error).ConfigureAwait(false);
    }

    /// <summary>Transforms the error while preserving the value type.</summary>
    public static async ValueTask<Result<T>> MapError<T>(
        this ValueTask<Result<T>> resultTask, Func<Error, Error> mapper)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.MapError(mapper);
    }

    /// <summary>Executes action regardless of success or failure.</summary>
    public static async ValueTask<Result<T>> Finally<T>(
        this ValueTask<Result<T>> resultTask, Action<Result<T>> action)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Finally(action);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  ValueTask<Result> (non-generic) extensions
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Forces handling of both success and failure cases.</summary>
    public static async ValueTask<TOut> Match<TOut>(
        this ValueTask<Result> resultTask,
        Func<TOut> onSuccess, Func<Error, TOut> onFailure)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Match(onSuccess, onFailure);
    }

    /// <summary>Executes a side effect on success.</summary>
    public static async ValueTask<Result> Tap(
        this ValueTask<Result> resultTask, Action onSuccess)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Tap(onSuccess);
    }

    /// <summary>Executes a side effect on failure.</summary>
    public static async ValueTask<Result> TapError(
        this ValueTask<Result> resultTask, Action<Error> onFailure)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.TapError(onFailure);
    }

    /// <summary>Validates a condition after success.</summary>
    public static async ValueTask<Result> Ensure(
        this ValueTask<Result> resultTask, Func<bool> predicate, Error error)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Ensure(predicate, error);
    }

    /// <summary>Transforms the error while preserving the success/failure state.</summary>
    public static async ValueTask<Result> MapError(
        this ValueTask<Result> resultTask, Func<Error, Error> mapper)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.MapError(mapper);
    }

    /// <summary>Executes action regardless of success or failure.</summary>
    public static async ValueTask<Result> Finally(
        this ValueTask<Result> resultTask, Action<Result> action)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Finally(action);
    }
}
