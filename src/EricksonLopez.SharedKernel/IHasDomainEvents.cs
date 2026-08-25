// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using EricksonLopez.Events.Contracts;

namespace EricksonLopez.SharedKernel;

/// <summary>
/// Defines the contract for domain objects capable of recording and releasing domain events.
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>
    /// Transfers and clears all pending domain events recorded by this instance.
    /// </summary>
    /// <returns>
    /// A read-only collection of pending domain events in emission order, or an empty collection if no events were recorded.
    /// </returns>
    IReadOnlyList<IDomainEvent> DrainDomainEvents();
}

