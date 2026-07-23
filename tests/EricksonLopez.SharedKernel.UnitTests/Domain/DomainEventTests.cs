namespace EricksonLopez.SharedKernel.UnitTests.Domain;

// --- Test double --------------------------------------------------------------

file sealed record TestDomainEvent(Guid Id) : IDomainEvent;

// --- Tests --------------------------------------------------------------------

/// <summary>
/// Validates the structural contract of <see cref="IDomainEvent"/>.
/// Tests verify only what the interface declares — not architectural conventions
/// (immutability, record usage, etc.) that the compiler does not enforce.
/// </summary>
public sealed class IDomainEventContractTests
{
    private static readonly Type InterfaceType = typeof(IDomainEvent);

    [Fact]
    public void IDomainEvent_ShouldBePublic()
    {
        // Assert
        InterfaceType.IsPublic.Should().BeTrue(
            "consumers in other assemblies must be able to implement it");
    }

    [Fact]
    public void IDomainEvent_ShouldBeAnInterface()
    {
        // Assert
        InterfaceType.IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IDomainEvent_ShouldResideInCorrectNamespace()
    {
        // Assert
        InterfaceType.Namespace.Should().Be("EricksonLopez.SharedKernel.Domain");
    }

    [Fact]
    public void IDomainEvent_ShouldDeclareNoMembers()
    {
        // Arrange
        var ownMethods = InterfaceType
            .GetMethods(System.Reflection.BindingFlags.Public
                        | System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.DeclaredOnly);

        // Assert
        ownMethods.Should().BeEmpty(
            "IDomainEvent is intentionally a marker interface with zero members");
    }

    [Fact]
    public void IDomainEvent_CanBeImplemented_ByConcreteType()
    {
        // Arrange & Act
        var @event = new TestDomainEvent(Guid.NewGuid());

        // Assert
        @event.Should().BeAssignableTo<IDomainEvent>(
            "any class or record can implement the marker interface");
    }

    [Fact]
    public void IDomainEvent_CanBeImplemented_ByMultipleDistinctTypes()
    {
        // Arrange
        IDomainEvent e1 = new TestDomainEvent(Guid.NewGuid());
        IDomainEvent e2 = new TestDomainEvent(Guid.NewGuid());

        // Assert
        e1.Should().BeAssignableTo<IDomainEvent>();
        e2.Should().BeAssignableTo<IDomainEvent>();
        e1.Should().NotBeSameAs(e2);
    }
}
