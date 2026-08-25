// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.SharedKernel.EntityFrameworkCore.Tests.Fakes;

using EricksonLopez.Events.Contracts;
using EricksonLopez.SharedKernel;
using EricksonLopez.SharedKernel.EntityFrameworkCore;

public class TestDispatcher : IDomainEventDispatcher
{
    public List<IDomainEvent> DispatchedEvents { get; } = new();

    public ValueTask DispatchAsync(IReadOnlyList<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DispatchedEvents.AddRange(domainEvents);
        return ValueTask.CompletedTask;
    }
}

public class ThrowingDispatcher : IDomainEventDispatcher
{
    public ValueTask DispatchAsync(IReadOnlyList<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Dispatched event handling failed intentionally.");
    }
}



