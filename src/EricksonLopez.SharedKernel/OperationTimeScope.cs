// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace EricksonLopez.SharedKernel;

/// <summary>
/// Manages the disposal and popping of the ambient operation timestamp stack.
/// </summary>
internal sealed class OperationTimeScope(
    AsyncLocal<ImmutableStack<OperationTimeContext.ScopeToken>?> stack,
    Guid scopeId) : IDisposable
{
    public void Dispose()
    {
        if (stack.Value is { IsEmpty: false } current)
        {
            if (current.Peek().Id == scopeId)
            {
                var next = current.Pop();
                stack.Value = next.IsEmpty ? null : next;
            }
            else
            {
                // Out-of-order or multi-context disposal: filter out this scope without corrupting remaining stack
                var remaining = new List<OperationTimeContext.ScopeToken>();
                bool found = false;
                foreach (var entry in current)
                {
                    if (entry.Id != scopeId)
                    {
                        remaining.Add(entry);
                    }
                    else
                    {
                        found = true;
                    }
                }

                if (found)
                {
                    // Stryker disable once all : Mathematically unreachable defensive branch; Peek() mismatch guarantees remaining.Count >= 1
                    if (remaining.Count == 0)
                    {
                        stack.Value = null;
                    }
                    else
                    {
                        var newStack = ImmutableStack<OperationTimeContext.ScopeToken>.Empty;
                        for (int i = remaining.Count - 1; i >= 0; i--)
                        {
                            newStack = newStack.Push(remaining[i]);
                        }
                        stack.Value = newStack;
                    }
                }
            }
        }
    }
}
