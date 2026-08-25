# ADR-031: Synchronous Dispatcher Policy in DomainEventsInterceptor

**Status:** Accepted
**Date:** 2026-08-19
**Author:** Erickson Lopez
**Supersedes:** N/A
**Related:** ADR-002 (Zero functional dependencies), ADR-007 (Native AOT)

---

## Context

`DomainEventsInterceptor` overrides both `SavingChanges` (synchronous) and `SavingChangesAsync` (asynchronous). When an `IDomainEventDispatcher` is registered, the synchronous path must dispatch domain events before the transaction commits.

The synchronous `SavingChanges` override currently implements dispatch as:

```csharp
_dispatcher.DispatchAsync(events, CancellationToken.None)
           .AsTask()
           .GetAwaiter()
           .GetResult();
```

This pattern — blocking a synchronous thread on an asynchronous `ValueTask` — is a known source of deadlock in environments with an active `SynchronizationContext`.

### Deadlock Mechanism

The classic deadlock occurs when:
1. The current thread calls `.GetAwaiter().GetResult()`, blocking and **holding** the `SynchronizationContext` context slot.
2. The `async` continuation inside `DispatchAsync` attempts to resume on the same `SynchronizationContext` to complete.
3. The continuation cannot acquire the context — it is blocked by step 1.
4. Deadlock: the thread waits for the task; the task waits for the thread.

**Affected environments:**
- Legacy ASP.NET (classic, not Core) — `AspNetSynchronizationContext`
- Windows Forms — `WindowsFormsSynchronizationContext`
- WPF — `DispatcherSynchronizationContext`

**Not affected:**
- ASP.NET Core with Kestrel default pipeline — no `SynchronizationContext` on thread-pool threads
- Console applications — no `SynchronizationContext` by default
- Any context where `SynchronizationContext.Current` is `null`

---

## Options Considered

### Option A — Document the limitation prominently (Selected)

Add explicit `/// <remarks>` with `WARNING — Deadlock Risk` in the XML documentation of `SavingChanges`, describing the affected environments and recommending `SavingChangesAsync` as the correct path.

**Pros:**
- Zero breaking change — no API surface modification
- Zero new dependencies — consistent with ADR-002
- Consumers on modern ASP.NET Core (Kestrel) are unaffected and see no warning noise

**Cons:**
- Does not prevent misuse — consumers can still call `SaveChanges()` without awareness

### Option B — Deprecate `SavingChanges` with `[Obsolete]`

Mark `SavingChanges` override with `[Obsolete("Prefer SaveChangesAsync. Sync dispatch may deadlock in SynchronizationContext-bound environments.", error: false)]`.

**Pros:**
- Compiler-level guidance — warns at call site

**Cons:**
- Breaking change in developer experience — generates CS0809 warnings in all consumers even on safe async pipelines
- EF Core infrastructure typically calls `SavingChanges` without consumer control — the warning would appear in EF Core internals, not in consumer code

### Option C — No-op the sync path (never dispatch on sync)

When `SavingChanges` is called (sync), skip dispatch entirely. Only dispatch in `SavingChangesAsync`.

**Pros:**
- Eliminates deadlock risk entirely

**Cons:**
- Silently drops events — consumers calling `SaveChanges()` lose all domain event dispatch without any warning or error
- A worse failure mode than a deadlock — domain invariants are silently violated

### Option D — Add `ConfigureAwait(false)` chain

Use `.ConfigureAwait(false)` throughout the dispatch chain to avoid resuming on the original `SynchronizationContext`.

**Analysis:** `ConfigureAwait(false)` must be applied at every `await` inside `DispatchAsync` by the consumer's implementation — not by this library. The library can only call `.AsTask().ConfigureAwait(false).GetAwaiter().GetResult()` which is still a blocking call and still risks deadlock if the `SynchronizationContext` captures within the consumer's `DispatchAsync` body.

**Conclusion:** Not a reliable solution from the library side alone.

---

## Decision

**Option A is selected.**

**Rationale:**

1. **Target environment is modern .NET:** The primary target is ASP.NET Core on Kestrel, which has no `SynchronizationContext` on thread-pool threads. The deadlock risk is non-existent in this environment.

2. **Zero breaking change:** The library is at v1.1.x. Option B would generate compiler warnings for all consumers with no actual benefit in the primary target environment.

3. **Consumer responsibility:** Consumers who use `SaveChanges()` (sync) with a dispatcher are responsible for ensuring their environment has no `SynchronizationContext`. The XML documentation now makes this risk explicit.

4. **Option C is the worst outcome:** Silent event loss is more dangerous than a detectable deadlock.

5. **ADR-002 compliance:** Adding any dependency (e.g., `Microsoft.Extensions.Logging.Abstractions` to log a warning) would violate ADR-002. Option A requires no dependency.

---

## Consequences

### Immediate

- `SavingChanges` XML doc now explicitly documents the deadlock risk and affected environments.
- `SavingChangesAsync` XML doc is marked as the **preferred path**.
- The implementation is unchanged — no behavioral modification.

### Future

- If EF Core adds a `SynchronizationContext` detection API in a future version, revisit to add a runtime guard that throws `InvalidOperationException` instead of deadlocking.
- If the ecosystem shifts toward mandatory async-only EF Core pipelines, revisit Option B.

---

## Guidance for Consumers

If you use `DomainEventsInterceptor` with a registered `IDomainEventDispatcher`:

**Correct (async pipeline — recommended):**
```csharp
await dbContext.SaveChangesAsync(cancellationToken);
```

**Potentially unsafe (sync, with active SynchronizationContext):**
```csharp
// Only safe if SynchronizationContext.Current is null
dbContext.SaveChanges();
```

**Safe check before sync usage:**
```csharp
// Verify no SynchronizationContext before using sync SaveChanges with a dispatcher
Debug.Assert(
    SynchronizationContext.Current is null,
    "DomainEventsInterceptor sync dispatch may deadlock with an active SynchronizationContext. Use SaveChangesAsync.");
```
