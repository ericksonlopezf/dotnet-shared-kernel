// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Threading;

namespace EricksonLopez.SharedKernel;

/// <summary>
/// Provides an ambient per-operation timestamp so domain logic, auditing, and events can stay deterministic
/// without threading a timestamp parameter through every entity method.
/// </summary>
public static class OperationTimeContext
{
    internal readonly record struct ScopeToken(Guid Id, DateTimeOffset Timestamp);

    private static readonly AsyncLocal<ImmutableStack<ScopeToken>?> _currentStack = new();

    /// <summary>
    /// Gets the current ambient operation timestamp if a scope is active; otherwise, <see langword="null"/>.
    /// </summary>
    public static DateTimeOffset? Current =>
        _currentStack.Value is { IsEmpty: false } stack ? stack.Peek().Timestamp : null;

    /// <summary>
    /// Gets the current ambient operation timestamp if available, or returns <see cref="TimeProvider.System"/>'s current UTC timestamp.
    /// </summary>
    /// <returns>The deterministic operation timestamp in UTC.</returns>
    public static DateTimeOffset CurrentOrUtcNow() =>
        Current ?? TimeProvider.System.GetUtcNow();

    /// <summary>
    /// Begins a new deterministic ambient operation time scope.
    /// </summary>
    /// <param name="timestamp">The frozen timestamp for the duration of this scope.</param>
    /// <returns>An <see cref="IDisposable"/> scope that restores the previous timestamp upon disposal.</returns>
    /// <exception cref="ArgumentException"><paramref name="timestamp"/> is equal to <c>default(DateTimeOffset)</c>.</exception>
    public static IDisposable BeginScope(DateTimeOffset timestamp)
    {
        if (timestamp == default)
        {
            throw new ArgumentException("Timestamp cannot be default(DateTimeOffset).", nameof(timestamp));
        }

        timestamp = timestamp.ToUniversalTime();

        var scopeId = Guid.NewGuid();
        var stack = _currentStack.Value ?? ImmutableStack<ScopeToken>.Empty;
        _currentStack.Value = stack.Push(new ScopeToken(scopeId, timestamp));
        return new OperationTimeScope(_currentStack, scopeId);
    }

    /// <summary>
    /// Resets the ambient operation time context for the current asynchronous execution context.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>⚠️ FOR TEST USE ONLY. DO NOT CALL IN PRODUCTION CODE.</strong>
    /// </para>
    /// <para>
    /// This method is intended exclusively for test isolation — it clears the entire ambient stack for the
    /// current async execution context. Calling this in production code will corrupt ambient timestamp state
    /// for any concurrent operation that shares the same execution context tree, causing silent data integrity
    /// failures in domain events and auditing.
    /// </para>
    /// <para>
    /// Proper cleanup in production code must always be achieved by disposing the <see cref="IDisposable"/>
    /// scope returned by <see cref="BeginScope"/>.
    /// </para>
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void Reset()
    {
        _currentStack.Value = null;
    }
}
