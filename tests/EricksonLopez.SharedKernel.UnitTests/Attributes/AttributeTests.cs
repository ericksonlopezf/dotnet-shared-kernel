using System.Reflection;
using EricksonLopez.SharedKernel.Attributes;

namespace EricksonLopez.SharedKernel.UnitTests.Attributes;

// ─── Test targets ─────────────────────────────────────────────────────────────

[ErrorDefinition]
file sealed class SampleErrorDefinitions { }

[ErrorDefinition]
file interface ISampleErrorInterface { }

file sealed class SampleFactory
{
    [ResultFactory]
    public static int Create() => 42;
}

// ─── ErrorDefinitionAttribute tests ──────────────────────────────────────────

public sealed class ErrorDefinitionAttributeTests
{
    private static readonly Type AttributeType = typeof(ErrorDefinitionAttribute);

    [Fact]
    public void ErrorDefinitionAttribute_ShouldExistAndBeAnAttribute()
    {
        // Assert
        AttributeType.IsClass.Should().BeTrue();
        AttributeType.IsSubclassOf(typeof(Attribute)).Should().BeTrue();
    }

    [Fact]
    public void ErrorDefinitionAttribute_ShouldBePublic()
    {
        // Assert
        AttributeType.IsPublic.Should().BeTrue();
    }

    [Fact]
    public void ErrorDefinitionAttribute_ShouldBeSealed()
    {
        // Assert
        AttributeType.IsSealed.Should().BeTrue();
    }

    [Fact]
    public void ErrorDefinitionAttribute_ShouldResideInCorrectNamespace()
    {
        // Assert
        AttributeType.Namespace.Should().Be("EricksonLopez.SharedKernel.Attributes");
    }

    [Fact]
    public void ErrorDefinitionAttribute_AttributeUsage_ShouldTargetClassStructInterface()
    {
        // Arrange
        var usage = AttributeType.GetCustomAttribute<AttributeUsageAttribute>();

        // Assert
        usage.Should().NotBeNull();
        usage!.ValidOn.Should().HaveFlag(AttributeTargets.Class);
        usage.ValidOn.Should().HaveFlag(AttributeTargets.Struct);
        usage.ValidOn.Should().HaveFlag(AttributeTargets.Interface);
    }

    [Fact]
    public void ErrorDefinitionAttribute_AttributeUsage_AllowMultiple_ShouldBeFalse()
    {
        // Arrange
        var usage = AttributeType.GetCustomAttribute<AttributeUsageAttribute>();

        // Assert
        usage!.AllowMultiple.Should().BeFalse("each type should declare errors exactly once");
    }

    [Fact]
    public void ErrorDefinitionAttribute_AttributeUsage_Inherited_ShouldBeFalse()
    {
        // Arrange
        var usage = AttributeType.GetCustomAttribute<AttributeUsageAttribute>();

        // Assert
        usage!.Inherited.Should().BeFalse();
    }

    [Fact]
    public void ErrorDefinitionAttribute_CanBeApplied_ToClass()
    {
        // Arrange
        var attr = typeof(SampleErrorDefinitions).GetCustomAttribute<ErrorDefinitionAttribute>();

        // Assert
        attr.Should().NotBeNull("the attribute must be retrievable from a decorated class");
    }

    [Fact]
    public void ErrorDefinitionAttribute_CanBeApplied_ToInterface()
    {
        // Arrange
        var attr = typeof(ISampleErrorInterface).GetCustomAttribute<ErrorDefinitionAttribute>();

        // Assert
        attr.Should().NotBeNull("the attribute must be retrievable from a decorated interface");
    }

    [Fact]
    public void ErrorDefinitionAttribute_Constructor_ShouldBeParameterless()
    {
        // Act — constructor must be callable without arguments
        var act = () => new ErrorDefinitionAttribute();

        // Assert
        act.Should().NotThrow();
    }
}

// ─── ResultFactoryAttribute tests ─────────────────────────────────────────────

public sealed class ResultFactoryAttributeTests
{
    private static readonly Type AttributeType = typeof(ResultFactoryAttribute);

    [Fact]
    public void ResultFactoryAttribute_ShouldExistAndBeAnAttribute()
    {
        // Assert
        AttributeType.IsClass.Should().BeTrue();
        AttributeType.IsSubclassOf(typeof(Attribute)).Should().BeTrue();
    }

    [Fact]
    public void ResultFactoryAttribute_ShouldBePublic()
    {
        // Assert
        AttributeType.IsPublic.Should().BeTrue();
    }

    [Fact]
    public void ResultFactoryAttribute_ShouldBeSealed()
    {
        // Assert
        AttributeType.IsSealed.Should().BeTrue();
    }

    [Fact]
    public void ResultFactoryAttribute_ShouldResideInCorrectNamespace()
    {
        // Assert
        AttributeType.Namespace.Should().Be("EricksonLopez.SharedKernel.Attributes");
    }

    [Fact]
    public void ResultFactoryAttribute_AttributeUsage_ShouldTargetMethod()
    {
        // Arrange
        var usage = AttributeType.GetCustomAttribute<AttributeUsageAttribute>();

        // Assert
        usage.Should().NotBeNull();
        usage!.ValidOn.Should().Be(AttributeTargets.Method,
            "factory marker is scoped to methods only");
    }

    [Fact]
    public void ResultFactoryAttribute_AttributeUsage_AllowMultiple_ShouldBeFalse()
    {
        // Arrange
        var usage = AttributeType.GetCustomAttribute<AttributeUsageAttribute>();

        // Assert
        usage!.AllowMultiple.Should().BeFalse();
    }

    [Fact]
    public void ResultFactoryAttribute_AttributeUsage_Inherited_ShouldBeFalse()
    {
        // Arrange
        var usage = AttributeType.GetCustomAttribute<AttributeUsageAttribute>();

        // Assert
        usage!.Inherited.Should().BeFalse();
    }

    [Fact]
    public void ResultFactoryAttribute_CanBeApplied_ToMethod()
    {
        // Arrange
        var method = typeof(SampleFactory).GetMethod("Create",
            BindingFlags.Public | BindingFlags.Static);
        var attr = method?.GetCustomAttribute<ResultFactoryAttribute>();

        // Assert
        attr.Should().NotBeNull("the attribute must be retrievable from a decorated method");
    }

    [Fact]
    public void ResultFactoryAttribute_Constructor_ShouldBeParameterless()
    {
        // Act
        var act = () => new ResultFactoryAttribute();

        // Assert
        act.Should().NotThrow();
    }
}
