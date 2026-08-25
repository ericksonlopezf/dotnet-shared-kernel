// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Reflection;
using AwesomeAssertions;
using EricksonLopez.DomainPrimitives;
using EricksonLopez.SharedKernel;
using NetArchTest.Rules;
using Xunit;

namespace EricksonLopez.SharedKernel.ArchitectureTests;

public class ArchitectureTests
{
    private const string SharedKernelNamespace = "EricksonLopez.SharedKernel";

    [Fact]
    public void SharedKernel_ShouldNot_HaveUnwantedDependencies()
    {
        // NOTE: Use ShouldNot().HaveDependencyOnAny() — NOT individual .And().NotHaveDependencyOn()
        // chains, which produce a double-negation that can evaluate incorrectly.
        var result = Types.InAssembly(typeof(Entity<>).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "MediatR",
                "Microsoft.EntityFrameworkCore",
                "Dapper",
                "EricksonLopez.Pagination",
                "Microsoft.Extensions",
                "Microsoft.AspNetCore",
                "System.Reflection.Emit",
                "Newtonsoft.Json",
                "System.Text.Json",
                "System.Collections.Concurrent"
            )
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "The SharedKernel must have zero dependencies on infrastructure, " +
                     "serialization, DI, ORMs, or concurrent collections.");
    }

    [Fact]
    public void SharedKernel_ShouldNot_ContainCastleProxyAwareness()
    {
        // Verifies that GetUnproxiedType() (removed in FINDING-002) has not been re-introduced.
        // Castle.Proxies is an ORM infrastructure concern that must not leak into the domain layer.
        var result = Types.InAssembly(typeof(Entity<>).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Castle")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Castle DynamicProxy awareness is an infrastructure concern — " +
                     "it must never appear in the SharedKernel domain layer.");
    }

    [Fact]
    public void SharedKernel_ShouldNot_DependOn_UnwantedEricksonLopezSiblingPackages()
    {
        var result = Types.InAssembly(typeof(Entity<>).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "EricksonLopez.Result",
                "EricksonLopez.DomainPrimitives",
                "EricksonLopez.Specification",
                "EricksonLopez.Outbox",
                "EricksonLopez.Mediator",
                "EricksonLopez.Mapper",
                "EricksonLopez.SqlBuilder"
            )
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "SharedKernel must not depend on application, mapping, outbox, or computation sibling packages.");
    }

    [Fact]
    public void DomainEvent_MustImplement_EventsContracts_IDomainEvent()
    {
        typeof(EricksonLopez.Events.Contracts.IDomainEvent).IsAssignableFrom(typeof(DomainEvent))
            .Should().BeTrue(because: "DomainEvent must implement EricksonLopez.Events.Contracts.IDomainEvent.");
    }

    [Fact]
    public void DomainEvent_MustBe_AbstractRecord()
    {
        typeof(DomainEvent).IsAbstract.Should().BeTrue();
        typeof(DomainEvent).IsClass.Should().BeTrue(
            because: "DomainEvent must compile to an abstract record class.");
    }

    [Fact]
    public void IHasDomainEvents_MustBe_Interface()
    {
        typeof(IHasDomainEvents).IsInterface.Should().BeTrue(
            because: "IHasDomainEvents must be an interface defining domain event collection.");
    }

    [Fact]
    public void IAggregateRoot_MustBe_Interface_And_Inherit_IHasDomainEvents()
    {
        typeof(IAggregateRoot).IsInterface.Should().BeTrue();
        typeof(IHasDomainEvents).IsAssignableFrom(typeof(IAggregateRoot)).Should().BeTrue(
            because: "IAggregateRoot must inherit IHasDomainEvents for polymorphic event access.");
    }

    [Fact]
    public void Entity_MustBe_AbstractClass()
    {
        typeof(Entity<>).IsAbstract.Should().BeTrue();
        typeof(Entity<>).IsClass.Should().BeTrue();
    }

    [Fact]
    public void AggregateRoot_MustInherit_Entity_And_Implement_IAggregateRoot()
    {
        typeof(AggregateRoot<>).BaseType!.GetGenericTypeDefinition()
            .Should().Be(typeof(Entity<>), because: "AggregateRoot must directly inherit from Entity.");

        typeof(IAggregateRoot).IsAssignableFrom(typeof(AggregateRoot<>)).Should().BeTrue(
            because: "AggregateRoot must implement IAggregateRoot.");

        typeof(IHasDomainEvents).IsAssignableFrom(typeof(AggregateRoot<>)).Should().BeTrue(
            because: "AggregateRoot must implement IHasDomainEvents.");
    }

    [Fact]
    public void ValueObject_MustBe_AbstractClass()
    {
        typeof(ValueObject).IsAbstract.Should().BeTrue();
        typeof(ValueObject).IsClass.Should().BeTrue(
            because: "ValueObject record must compile to an abstract record class.");
    }

    [Fact]
    public void IStrongId_MustBe_GenericInterface()
    {
        typeof(IStrongId<,>).IsInterface.Should().BeTrue(
            because: "IStrongId must be an interface defining strongly-typed identity.");
    }

    [Fact]
    public void Entity_IdProperty_MustNotHaveSetter()
    {
        var idProp = typeof(Entity<>).GetProperty("Id");
        idProp.Should().NotBeNull();

        // Ensure no setter exists (it is pure getter-only: get;)
        var setMethod = idProp!.GetSetMethod(nonPublic: true);
        setMethod.Should().BeNull(because: "Entity.Id must be pure getter-only and strictly immutable.");
    }

    [Fact]
    public void DomainEvent_Properties_MustNotHaveSetter()
    {
        var idProp = typeof(DomainEvent).GetProperty("Id");
        idProp.Should().NotBeNull();
        idProp!.GetSetMethod(nonPublic: true).Should().BeNull(
            because: "DomainEvent.Id must be pure getter-only.");

        var occurredAtProp = typeof(DomainEvent).GetProperty("OccurredAt");
        occurredAtProp.Should().NotBeNull();
        occurredAtProp!.GetSetMethod(nonPublic: true).Should().BeNull(
            because: "DomainEvent.OccurredAt must be pure getter-only.");

        var eventIdProp = typeof(DomainEvent).GetProperty("EventId");
        eventIdProp.Should().NotBeNull();
        eventIdProp!.GetSetMethod(nonPublic: true).Should().BeNull(
            because: "DomainEvent.EventId must be pure getter-only.");

        var occurredOnProp = typeof(DomainEvent).GetProperty("OccurredOn");
        occurredOnProp.Should().NotBeNull();
        occurredOnProp!.GetSetMethod(nonPublic: true).Should().BeNull(
            because: "DomainEvent.OccurredOn must be pure getter-only.");
    }

    [Fact]
    public void AggregateRoot_DrainDomainEvents_MustReturnReadOnlyList()
    {
        var method = typeof(AggregateRoot<>).GetMethod("DrainDomainEvents", BindingFlags.Public | BindingFlags.Instance);
        method.Should().NotBeNull();
        method!.ReturnType.GetGenericTypeDefinition()
            .Should().Be(typeof(IReadOnlyList<>), because: "DrainDomainEvents must return a read-only list contract.");
    }

    [Fact]
    public void IHasDomainEvents_DrainDomainEvents_MustReturnReadOnlyList()
    {
        var method = typeof(IHasDomainEvents).GetMethod("DrainDomainEvents");
        method.Should().NotBeNull();
        method!.ReturnType.GetGenericTypeDefinition()
            .Should().Be(typeof(IReadOnlyList<>), because: "IHasDomainEvents.DrainDomainEvents must return a read-only list contract.");
    }
}



