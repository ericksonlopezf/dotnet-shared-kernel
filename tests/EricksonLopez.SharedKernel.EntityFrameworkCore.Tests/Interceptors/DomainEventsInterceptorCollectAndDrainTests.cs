// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.SharedKernel;
using EricksonLopez.SharedKernel.EntityFrameworkCore;
using EricksonLopez.SharedKernel.EntityFrameworkCore.Tests.Builders;
using EricksonLopez.SharedKernel.EntityFrameworkCore.Tests.Fakes;
using EricksonLopez.SharedKernel.EntityFrameworkCore.Tests.Fixtures;
using EricksonLopez.SharedKernel.TestingUtilities.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NSubstitute;
using Xunit;

namespace EricksonLopez.SharedKernel.EntityFrameworkCore.Tests.Interceptors;

public class DomainEventsInterceptorCollectAndDrainTests
{
    private static DbContextOptions<TestSharedKernelDbContext> CreateInMemoryOptions()
        => TestDbContextFactory.CreateInMemoryOptions<TestSharedKernelDbContext>();

    private static (IDomainEventDispatcher Dispatcher, DomainEventsInterceptor Interceptor) CreateInterceptorWithMockDispatcher()
    {
        var dispatcher = Substitute.For<IDomainEventDispatcher>();
        dispatcher.DispatchAsync(Arg.Any<IReadOnlyList<IDomainEvent>>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.CompletedTask);
        var interceptor = new DomainEventsInterceptor(dispatcher);
        return (dispatcher, interceptor);
    }

    #region Guard & Defensive Null Handling

    [Fact]
    public void SavingChanges_WithNullEventData_ThrowsArgumentNullException()
    {
        var interceptor = new DomainEventsInterceptor();

        var act = () => interceptor.SavingChanges(null!, new InterceptionResult<int>());

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("eventData");
    }

    [Fact]
    public async Task SavingChangesAsync_WithNullEventData_ThrowsArgumentNullException()
    {
        var interceptor = new DomainEventsInterceptor();

        var act = async () => await interceptor.SavingChangesAsync(null!, new InterceptionResult<int>());

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("eventData");
    }

    [Fact]
    public void SavingChanges_WithNullContextInEventData_ReturnsBaseResult()
    {
        // DbContextEventData has no public constructor allowing null Context; internal constructor encapsulation
        // prevents direct instantiation with null. RuntimeHelpers.GetUninitializedObject creates a defensive stub
        // to verify that the guard (eventData.Context is not null) handles null contexts safely without throwing.
        var (dispatcher, interceptor) = CreateInterceptorWithMockDispatcher();
        var eventData = (DbContextEventData)RuntimeHelpers.GetUninitializedObject(typeof(DbContextEventData));

        var result = interceptor.SavingChanges(eventData, new InterceptionResult<int>());

        result.Should().Be(default(InterceptionResult<int>));
        dispatcher.DidNotReceive().DispatchAsync(Arg.Any<IReadOnlyList<IDomainEvent>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SavingChangesAsync_WithNullContextInEventData_ReturnsBaseResult()
    {
        // DbContextEventData has no public constructor allowing null Context; internal constructor encapsulation
        // prevents direct instantiation with null. RuntimeHelpers.GetUninitializedObject creates a defensive stub
        // to verify that the guard (eventData.Context is not null) handles null contexts safely without throwing.
        var (dispatcher, interceptor) = CreateInterceptorWithMockDispatcher();
        var eventData = (DbContextEventData)RuntimeHelpers.GetUninitializedObject(typeof(DbContextEventData));

        var result = await interceptor.SavingChangesAsync(eventData, new InterceptionResult<int>());

        result.Should().Be(default(InterceptionResult<int>));
        await dispatcher.DidNotReceive().DispatchAsync(Arg.Any<IReadOnlyList<IDomainEvent>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void CollectAndDrainEvents_WithNullContext_ThrowsArgumentNullException()
    {
        var act = () => DomainEventsInterceptor.CollectAndDrainEvents(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("context");
    }

    #endregion

    #region CollectAndDrainEvents Core Behavior

    [Fact]
    public async Task CollectAndDrainEvents_WithNoEntitiesWithEvents_ReturnsEmptyArray()
    {
        var options = CreateInMemoryOptions();
        await using var context = new TestSharedKernelDbContext(options);

        context.PlainEntities.Add(new PlainEntity { Id = 1, Description = "Test" });

        var events = DomainEventsInterceptor.CollectAndDrainEvents(context);
        events.Should().BeSameAs(Array.Empty<IDomainEvent>());
    }

    [Fact]
    public async Task CollectAndDrainEvents_WithMultipleEntitiesWithEvents_DrainsAllEvents()
    {
        var options = CreateInMemoryOptions();
        await using var context = new TestSharedKernelDbContext(options);

        var cust1 = new CustomerAggregate(CustomerId.New(), "User 1");
        var cust2 = new CustomerAggregate(CustomerId.New(), "User 2");

        context.Customers.AddRange(cust1, cust2);

        var events = DomainEventsInterceptor.CollectAndDrainEvents(context);
        events.Should().HaveCount(2);
        events[0].Should().BeOfType<CustomerRegisteredEvent>();
        events[1].Should().BeOfType<CustomerRegisteredEvent>();

        // Subsequent drain is empty because they were detached
        var eventsSecond = DomainEventsInterceptor.CollectAndDrainEvents(context);
        eventsSecond.Should().BeSameAs(Array.Empty<IDomainEvent>());
    }

    [Theory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Unchanged)]
    public async Task CollectAndDrainEvents_WithEntitiesInVariousTrackedEntityStates_DrainsEventsFromAllTrackedEntities(
        EntityState targetState)
    {
        var options = CreateInMemoryOptions();
        await using var context = new TestSharedKernelDbContext(options);

        var customer = new CustomerAggregate(CustomerId.New(), "Tracked State User");
        context.Customers.Attach(customer);
        context.Entry(customer).State = targetState;

        var events = DomainEventsInterceptor.CollectAndDrainEvents(context);

        events.Should().ContainSingle()
            .Which.Should().BeOfType<CustomerRegisteredEvent>();
        customer.DrainDomainEvents().Should().BeEmpty();
    }

    [Fact]
    public async Task CollectAndDrainEvents_WithDetachedEntity_DoesNotDrainEvents()
    {
        var options = CreateInMemoryOptions();
        await using var context = new TestSharedKernelDbContext(options);

        var customer = new CustomerAggregate(CustomerId.New(), "Detached User");
        context.Customers.Attach(customer);
        context.Entry(customer).State = EntityState.Detached;

        var events = DomainEventsInterceptor.CollectAndDrainEvents(context);

        events.Should().BeSameAs(Array.Empty<IDomainEvent>());
        customer.DrainDomainEvents().Should().ContainSingle();
    }

    [Theory]
    [InlineData(10, 10)]       // 100 events (Baseline)
    [InlineData(100, 10)]      // 1,000 events (1K Fast Tier)
    public async Task CollectAndDrainEvents_WithVaryingHighVolumeAggregates_DrainsAllEventsAndClearsAllBuffers(
        int aggregateCount,
        int eventsPerAggregate)
    {
        await ExecuteCollectAndDrainHighVolumeTestAsync(aggregateCount, eventsPerAggregate);
    }

    [Theory]
    [Trait("Category", "Stress")]
    [InlineData(1_000, 10)]    // 10,000 events (10K Stress Tier)
    [InlineData(10_000, 10)]   // 100,000 events (100K High Stress Tier)
    [InlineData(100_000, 10)]  // 1,000,000 events (1M Extreme Stress Tier)
    public async Task CollectAndDrainEvents_WithExtremeHighVolumeAggregates_DrainsAllEventsAndClearsAllBuffers(
        int aggregateCount,
        int eventsPerAggregate)
    {
        await ExecuteCollectAndDrainHighVolumeTestAsync(aggregateCount, eventsPerAggregate);
    }

    private static async Task ExecuteCollectAndDrainHighVolumeTestAsync(int aggregateCount, int eventsPerAggregate)
    {
        var options = CreateInMemoryOptions();
        await using var context = new TestSharedKernelDbContext(options);

        var builder = AggregateTestBuilder.Create()
            .WithAggregateCount(aggregateCount)
            .WithEventsPerAggregate(eventsPerAggregate)
            .WithNamePrefix("User");
            
        var aggregates = builder.Build();

        context.Customers.AddRange(aggregates);

        var events = DomainEventsInterceptor.CollectAndDrainEvents(context);

        builder.VerifyDrainedEvents(aggregates, events);

        foreach (var aggregate in aggregates)
        {
            // Verify the buffer is completely cleared
            aggregate.DrainDomainEvents().Should().BeEmpty();
        }

        var subsequentDrain = DomainEventsInterceptor.CollectAndDrainEvents(context);
        subsequentDrain.Should().BeEmpty();
    }

    #endregion
}
