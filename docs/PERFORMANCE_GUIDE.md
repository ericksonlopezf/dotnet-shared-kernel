# Performance Guide — EricksonLopez.SharedKernel

Performance characteristics of the library. Based on source code analysis and benchmarks with BenchmarkDotNet.

---

## Performance Characteristics by Design

### 1. Lazy Allocation of Domain Events

```csharp
// The internal _domainEvents list is NEVER allocated until the first event
public abstract class AggregateRoot<TId> : Entity<TId>
{
    private List<IDomainEvent>? _domainEvents; // null until first event

    public IReadOnlyCollection<IDomainEvent> DomainEvents =>
        _domainEvents?.AsReadOnly()
        ?? (IReadOnlyCollection<IDomainEvent>)Array.Empty<IDomainEvent>();
}
```

**Implication:** Hydrating thousands of aggregates from the database (without invoking business methods) produces **zero bytes of heap allocation** for the events collection.

| Scenario | Heap Allocation (events) |
|---|---|
| Adding aggregate to DbContext | 0 bytes |
| Calling `DomainEvents` with no events | 0 bytes (static Array.Empty) |
| First `RaiseDomainEvent` | ~32 bytes (empty List<T>) + event |
| Subsequent calls | ~16 bytes per event (List.Add) |

---

### 2. GetHashCode — No Boxing for Structs

```csharp
public override int GetHashCode()
{
    if (IsTransient())
        return base.GetHashCode();

    return HashCode.Combine(
        GetType(),
        EqualityComparer<TId>.Default.GetHashCode(Id)
    );
}
```

`EqualityComparer<TId>.Default` for `TId : struct, IEquatable<TId>` (e.g. `Guid`, `int`, `record struct`) calls `IEquatable<T>.GetHashCode()` directly — **no boxing**. For `string`, it is also optimized.

---

### 3. IsTransient — O(1) with no allocation

```csharp
public bool IsTransient()
    => EqualityComparer<TId>.Default.Equals(Id, default!);
```

- O(1) — direct comparison
- No boxing for `struct` types implementing `IEquatable<T>`
- No allocation

---

### 4. NativeAOT — Millisecond Startup

The library has `IsAotCompatible=true` and `IsTrimmable=true` on all TFMs.

**Impact:**
- No dynamic reflection at runtime → complete static analysis at compile time
- No runtime code generation → no JIT warm-up
- Container startup: typical JIT ~300-500ms → NativeAOT ~20-50ms
- Critical for scale-to-zero in Kubernetes / Azure Container Apps

---

### 5. ClearDomainEvents — O(n) where n = number of events

```csharp
public void ClearDomainEvents() => _domainEvents?.Clear();
```

- If no events (`_domainEvents == null`): O(1) — only the null check
- If events exist: O(n) — `List<T>.Clear()` (sets Count=0, clears references)
- Does **not** reallocate the list — reuses the cleared list for the next operation

---

## Reference Benchmarks

Benchmarks are located in `benchmarks/EricksonLopez.SharedKernel.Benchmarks/SharedKernelBenchmarks.cs`.

### Running Benchmarks

```bash
dotnet run --project benchmarks/EricksonLopez.SharedKernel.Benchmarks/EricksonLopez.SharedKernel.Benchmarks.csproj -c Release
```

### Included Benchmarks

| Benchmark | What it measures |
|---|---|
| `EntityEquality` | Cost of `Entity<Guid>.Equals()` |
| `EntityGetHashCode` | Cost of `Entity<Guid>.GetHashCode()` |
| `AggregateDomainEventsAccessNoEvents` | `DomainEvents` access with no events (lazy alloc) |

---

## Performance Recommendations for Consumers

### For read-only collections (queries)

```csharp
// ✅ Use LINQ directly on DomainEvents — don't copy if not needed
if (aggregate.DomainEvents.Any())
    // process

// ❌ Don't copy unnecessarily if you're only iterating
var copy = aggregate.DomainEvents.ToList(); // unnecessary alloc if you only read
```

### For UnitOfWork (write side)

```csharp
// ✅ Take snapshot only when events exist
var events = aggregate.DomainEvents.Count > 0
    ? aggregate.DomainEvents.ToList()
    : null;

aggregate.ClearDomainEvents();

if (events is not null)
    foreach (var ev in events)
        await _publisher.Publish(ev);
```

### For Strongly Typed Ids

```csharp
// ✅ record struct — stack allocation, no boxing in GetHashCode/Equals
public readonly record struct OrderId(Guid Value);

// ❌ class — heap allocation for each Id
public class OrderId { public Guid Value { get; } }
```

---

## Expected Memory Profile

| Type | Approximate heap size |
|---|---|
| `Entity<Guid>` (no extra fields) | ~24 bytes (object header + Guid) |
| `AggregateRoot<Guid>` with no events | ~32 bytes (object header + Guid + null ptr) |
| `AggregateRoot<Guid>` with 1 event | ~32 + ~32 (List) + ~16 (entry) + event |
| `IDomainEvent` (sealed record, 1 Guid) | ~24 bytes |
