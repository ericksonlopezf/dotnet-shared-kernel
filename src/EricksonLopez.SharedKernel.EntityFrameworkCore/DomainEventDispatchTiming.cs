// Copyright © Erickson Lopez. MIT License.

namespace EricksonLopez.SharedKernel.EntityFrameworkCore;

/// <summary>
/// Defines the execution timing for domain event dispatching relative to database transaction commit in Entity Framework Core.
/// </summary>
public enum DomainEventDispatchTiming
{
    /// <summary>
    /// Dispatches domain events after the database transaction has committed (<see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()"/>).
    /// </summary>
    AfterCommit = 0,

    /// <summary>
    /// Dispatches domain events before the database transaction commits (<see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()"/>),
    /// allowing transaction rollback if event dispatching fails.
    /// </summary>
    /// <remarks>
    /// CAUTION: If EF Core retry execution strategies (e.g. <c>EnableRetryOnFailure</c>) are enabled,
    /// drained events restored during transient transaction failure will be re-dispatched on subsequent retry attempts.
    /// External integration event publishing should use the Transactional Outbox pattern or <see cref="AfterCommit"/> timing instead.
    /// </remarks>
    BeforeCommit = 1
}
