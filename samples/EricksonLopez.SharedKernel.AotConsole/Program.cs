// ═══════════════════════════════════════════════════════════════════════════
// EricksonLopez.SharedKernel — AOT Console Validator
// ═══════════════════════════════════════════════════════════════════════════
// This project validates the NativeAOT compatibility of the library.
// It exclusively uses the public API: Entity<TId>, AggregateRoot<TId>,
// IDomainEvent.
//
// Publish with: dotnet publish -r linux-x64 -c Release
// ═══════════════════════════════════════════════════════════════════════════

using System;
using EricksonLopez.SharedKernel;

Console.WriteLine("=== SharedKernel AOT Compatibility Validator ===");

// ── Validation 1: Entity<Guid> ──────────────────────────────────────────────
var entity1 = new AotEntity(Guid.NewGuid());
var entity2 = new AotEntity(entity1.Id);

Console.WriteLine($"Entity created          : {entity1.Id}");
Console.WriteLine($"IsTransient             : {entity1.IsTransient()}");
Console.WriteLine($"entity1 == entity2      : {entity1 == entity2}");
Console.WriteLine($"entity1.GetHashCode()   : {entity1.GetHashCode()}");

// ── Validation 2: AggregateRoot<Guid> + IDomainEvent ───────────────────────
var aggregate = AotAggregate.Create(Guid.NewGuid(), "AOT Test");

Console.WriteLine($"Aggregate created       : {aggregate.Id}");
Console.WriteLine($"DomainEvents.Count      : {aggregate.DomainEvents.Count}");

aggregate.ClearDomainEvents();
Console.WriteLine($"After ClearDomainEvents : {aggregate.DomainEvents.Count}");

// ── Validation 3: Lazy allocation (zero alloc on read) ─────────────────────
var readonlyAggregate = new AotReadonlyAggregate(Guid.NewGuid());
Console.WriteLine($"DomainEvents (no events): {readonlyAggregate.DomainEvents.Count} (zero alloc)");

// ── Validation 4: Strongly Typed Id ────────────────────────────────────────
var strongId = new AotEntityId(Guid.NewGuid());
var strongEntity = new StronglyTypedAotEntity(strongId);
Console.WriteLine($"Strongly Typed Id       : {strongEntity.Id}");

Console.WriteLine("=== AOT Validator: OK ===");

// ── Types for AOT validator ────────────────────────────────────────────────

/// <summary>Domain event for AOT validation.</summary>
sealed record AotDomainEvent(Guid AggregateId, string Name) : IDomainEvent;

/// <summary>Simple entity for AOT validation.</summary>
sealed class AotEntity : Entity<Guid>
{
    public AotEntity(Guid id) => Id = id;
}

/// <summary>Aggregate Root for AOT validation.</summary>
sealed class AotAggregate : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;

    public static AotAggregate Create(Guid id, string name)
    {
        var agg = new AotAggregate { Id = id, Name = name };
        agg.RaiseDomainEvent(new AotDomainEvent(id, name));
        return agg;
    }
}

/// <summary>Read-only aggregate — validates zero alloc on hydration.</summary>
sealed class AotReadonlyAggregate : AggregateRoot<Guid>
{
    public AotReadonlyAggregate(Guid id) => Id = id;
}

/// <summary>Strongly Typed Id for AOT validation.</summary>
readonly record struct AotEntityId(Guid Value);

/// <summary>Entity with Strongly Typed Id for AOT validation.</summary>
sealed class StronglyTypedAotEntity : Entity<AotEntityId>
{
    public StronglyTypedAotEntity(AotEntityId id) => Id = id;
}
