// ═══════════════════════════════════════════════════════════════════════════
// EricksonLopez.SharedKernel — AotTest
// ═══════════════════════════════════════════════════════════════════════════
// AOT compatibility test of the library.
// It exclusively uses the public API: Entity<TId>, AggregateRoot<TId>,
// IDomainEvent.
// ═══════════════════════════════════════════════════════════════════════════
#pragma warning disable CS1591 // Missing XML comment — test/sample project

using System;
using EricksonLopez.SharedKernel;

namespace AotTest;

/// <summary>Test domain event.</summary>
public sealed record UserCreated(Guid UserId, string Name) : IDomainEvent;

/// <summary>User Aggregate Root for AOT test.</summary>
public sealed class User : AggregateRoot<Guid>
{
    public string Name { get; private set; }

    private User(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public static User Create(Guid id, string name)
    {
        var user = new User(id, name);
        user.RaiseDomainEvent(new UserCreated(id, name));
        return user;
    }
}

/// <summary>User entity for AOT test.</summary>
public sealed class UserEntity : Entity<Guid>
{
    public string Name { get; }

    public UserEntity(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}

/// <summary>Strongly Typed Id for AOT test.</summary>
public readonly record struct UserId(Guid Value);

/// <summary>Entity with Strongly Typed Id for AOT test.</summary>
public sealed class StronglyTypedUser : Entity<UserId>
{
    public string Name { get; }

    public StronglyTypedUser(UserId id, string name)
    {
        Id = id;
        Name = name;
    }
}

public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== AotTest: EricksonLopez.SharedKernel ===");

        // ── Test 1: AggregateRoot + RaiseDomainEvent ────────────────────────
        var user = User.Create(Guid.NewGuid(), "Erick");
        Console.WriteLine($"User.Id             : {user.Id}");
        Console.WriteLine($"User.Name           : {user.Name}");
        Console.WriteLine($"DomainEvents.Count  : {user.DomainEvents.Count}");
        Console.WriteLine($"IsTransient         : {user.IsTransient()}");

        user.ClearDomainEvents();
        Console.WriteLine($"After Clear         : {user.DomainEvents.Count}");

        // ── Test 2: Entity equality ─────────────────────────────────────────
        var id = Guid.NewGuid();
        var e1 = new UserEntity(id, "Alice");
        var e2 = new UserEntity(id, "Alice Alias");
        Console.WriteLine($"e1 == e2            : {e1 == e2}");
        Console.WriteLine($"e1.GetHashCode()    : {e1.GetHashCode()}");

        // ── Test 3: IsTransient ─────────────────────────────────────────────
        var transient = new UserEntity(Guid.Empty, "Transient");
        Console.WriteLine($"transient.IsTransient: {transient.IsTransient()}");

        // ── Test 4: Strongly Typed Id ───────────────────────────────────────
        var uid = new UserId(Guid.NewGuid());
        var stu = new StronglyTypedUser(uid, "Bob");
        Console.WriteLine($"StronglyTyped Id    : {stu.Id}");

        Console.WriteLine("=== AotTest: PASS ===");
    }
}
