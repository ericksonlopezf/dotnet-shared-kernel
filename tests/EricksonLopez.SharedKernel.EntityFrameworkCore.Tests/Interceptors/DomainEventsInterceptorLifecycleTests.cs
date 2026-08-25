// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
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
using NSubstitute;
using Xunit;

namespace EricksonLopez.SharedKernel.EntityFrameworkCore.Tests.Interceptors;

public class DomainEventsInterceptorLifecycleTests
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

    #region SavingChanges & SavingChangesAsync Lifecycle

    [Theory]
    [InlineData(10, 10)]       // 100 events (Baseline)
    [InlineData(100, 10)]      // 1,000 events (1K Fast Tier)
    public async Task SavingChangesAsync_WithVaryingHighVolumeAggregates_DispatchesAllEventsAtomically(
        int aggregateCount,
        int eventsPerAggregate)
    {
        await ExecuteSavingChangesHighVolumeTestAsync(aggregateCount, eventsPerAggregate);
    }

    [Theory]
    [Trait("Category", "Stress")]
    [InlineData(1_000, 10)]    // 10,000 events (10K Stress Tier)
    [InlineData(10_000, 10)]   // 100,000 events (100K High Stress Tier)
    [InlineData(100_000, 10)]  // 1,000,000 events (1M Extreme Stress Tier)
    public async Task SavingChangesAsync_WithExtremeHighVolumeAggregates_DispatchesAllEventsAtomically(
        int aggregateCount,
        int eventsPerAggregate)
    {
        await ExecuteSavingChangesHighVolumeTestAsync(aggregateCount, eventsPerAggregate);
    }

    private static async Task ExecuteSavingChangesHighVolumeTestAsync(int aggregateCount, int eventsPerAggregate)
    {
        var (dispatcher, interceptor) = CreateInterceptorWithMockDispatcher();
        var options = CreateInMemoryOptions();
        var expectedTotalEvents = aggregateCount * eventsPerAggregate;

        await using (var context = new TestSharedKernelDbContext(options, interceptor))
        {
            var aggregates = AggregateTestBuilder.Create()
                .WithAggregateCount(aggregateCount)
                .WithEventsPerAggregate(eventsPerAggregate)
                .WithNamePrefix("HighVolume User")
                .Build();

            context.Customers.AddRange(aggregates);
            await context.SaveChangesAsync();
        }

        await dispatcher.Received(1).DispatchAsync(
            Arg.Is<IReadOnlyList<IDomainEvent>>(events =>
                events.Count == expectedTotalEvents &&
                events.OfType<CustomerRegisteredEvent>().Count() == aggregateCount &&
                events.OfType<CustomerNameUpdatedEvent>().Count() == expectedTotalEvents - aggregateCount),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SavingChanges_Synchronous_PersistsAndDispatchesEvents()
    {
        // Note: DomainEventsInterceptor.SavingChanges runs synchronously, but invokes IDomainEventDispatcher.DispatchAsync.
        // The DbContext scope is explicitly disposed before asserting dispatcher reception to verify that all events
        // were detached and dispatched prior to the persistence transaction completion.
        var (dispatcher, interceptor) = CreateInterceptorWithMockDispatcher();
        var options = CreateInMemoryOptions();
        var customerId = CustomerId.New();

        using (var context = new TestSharedKernelDbContext(options, interceptor))
        {
            var customer = new CustomerAggregate(customerId, "Sync User");
            context.Customers.Add(customer);

            context.SaveChanges();
        }

        await dispatcher.Received(1).DispatchAsync(
            Arg.Is<IReadOnlyList<IDomainEvent>>(events =>
                events.Count == 1 &&
                events.OfType<CustomerRegisteredEvent>().Any(cre => cre.CustomerId == customerId)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SavingChangesAsync_Asynchronous_PersistsAndDispatchesEvents()
    {
        var (dispatcher, interceptor) = CreateInterceptorWithMockDispatcher();
        var options = CreateInMemoryOptions();
        var customerId = CustomerId.New();

        await using (var context = new TestSharedKernelDbContext(options, interceptor))
        {
            var customer = new CustomerAggregate(customerId, "Async User");
            context.Customers.Add(customer);

            await context.SaveChangesAsync();
        }

        await dispatcher.Received(1).DispatchAsync(
            Arg.Is<IReadOnlyList<IDomainEvent>>(events =>
                events.Count == 1 &&
                events.OfType<CustomerRegisteredEvent>().Any(cre => cre.CustomerId == customerId)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void SavingChanges_Synchronous_WithoutDispatcher_SucceedsWithoutError()
    {
        var interceptor = new DomainEventsInterceptor(null);
        var options = CreateInMemoryOptions();
        var customerId = CustomerId.New();
        var customer = new CustomerAggregate(customerId, "Sync No Dispatcher");

        using (var context = new TestSharedKernelDbContext(options, interceptor))
        {
            context.Customers.Add(customer);
            var act = () => context.SaveChanges();
            act.Should().NotThrow();
        }

        customer.DrainDomainEvents().Should().BeEmpty(
            because: "DomainEventsInterceptor must drain aggregate event buffers even when dispatcher is null.");
    }

    [Fact]
    public async Task SavingChangesAsync_WithoutDispatcher_SucceedsWithoutError()
    {
        var interceptor = new DomainEventsInterceptor(null);
        var options = CreateInMemoryOptions();
        var customerId = CustomerId.New();
        var customer = new CustomerAggregate(customerId, "No Dispatcher");

        await using (var context = new TestSharedKernelDbContext(options, interceptor))
        {
            context.Customers.Add(customer);
            var act = async () => await context.SaveChangesAsync();
            await act.Should().NotThrowAsync();
        }

        customer.DrainDomainEvents().Should().BeEmpty(
            because: "DomainEventsInterceptor must drain aggregate event buffers even when dispatcher is null.");
    }

    [Fact]
    public void SavingChanges_HighVolumeBatch_WithoutDispatcher_DrainsAllEventsAndSucceeds()
    {
        var interceptor = new DomainEventsInterceptor(null);
        var options = CreateInMemoryOptions();
        const int aggregateCount = 50;
        const int eventsPerAggregate = 10;

        var aggregates = AggregateTestBuilder.Create()
            .WithAggregateCount(aggregateCount)
            .WithEventsPerAggregate(eventsPerAggregate)
            .WithNamePrefix("SyncNoDispatcher User")
            .Build();

        using (var context = new TestSharedKernelDbContext(options, interceptor))
        {
            context.Customers.AddRange(aggregates);
            var act = () => context.SaveChanges();
            act.Should().NotThrow();
        }

        foreach (var aggregate in aggregates)
        {
            aggregate.DrainDomainEvents().Should().BeEmpty(
                because: "DomainEventsInterceptor must drain aggregate event buffers for all batch aggregates even without a dispatcher.");
        }
    }

    [Fact]
    public async Task SavingChangesAsync_HighVolumeBatch_WithoutDispatcher_DrainsAllEventsAndSucceeds()
    {
        var interceptor = new DomainEventsInterceptor(null);
        var options = CreateInMemoryOptions();
        const int aggregateCount = 100;
        const int eventsPerAggregate = 10;

        var aggregates = AggregateTestBuilder.Create()
            .WithAggregateCount(aggregateCount)
            .WithEventsPerAggregate(eventsPerAggregate)
            .WithNamePrefix("NoDispatcher User")
            .Build();

        await using (var context = new TestSharedKernelDbContext(options, interceptor))
        {
            context.Customers.AddRange(aggregates);
            var act = async () => await context.SaveChangesAsync();
            await act.Should().NotThrowAsync();
        }

        foreach (var aggregate in aggregates)
        {
            aggregate.DrainDomainEvents().Should().BeEmpty(
                because: "DomainEventsInterceptor must drain aggregate event buffers for all high-volume aggregates even without a dispatcher.");
        }
    }

    [Fact]
    public void SavingChanges_Synchronous_WithNoEvents_DoesNotInvokeDispatcher()
    {
        var (dispatcher, interceptor) = CreateInterceptorWithMockDispatcher();
        var options = CreateInMemoryOptions();

        using var context = new TestSharedKernelDbContext(options, interceptor);
        context.PlainEntities.Add(new PlainEntity { Id = 10, Description = "No events" });

        context.SaveChanges();

        dispatcher.DidNotReceive().DispatchAsync(
            Arg.Any<IReadOnlyList<IDomainEvent>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SavingChangesAsync_WithNoEvents_DoesNotInvokeDispatcher()
    {
        var (dispatcher, interceptor) = CreateInterceptorWithMockDispatcher();
        var options = CreateInMemoryOptions();

        await using var context = new TestSharedKernelDbContext(options, interceptor);
        context.PlainEntities.Add(new PlainEntity { Id = 20, Description = "No events async" });

        await context.SaveChangesAsync();

        await dispatcher.DidNotReceive().DispatchAsync(
            Arg.Any<IReadOnlyList<IDomainEvent>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SavingChangesAsync_WithCancellationToken_PropagatesCancellation()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var dispatcher = new TestDispatcher();
        var interceptor = new DomainEventsInterceptor(dispatcher);
        var options = CreateInMemoryOptions();

        await using var context = new TestSharedKernelDbContext(options, interceptor);
        var customer = new CustomerAggregate(CustomerId.New(), "Cancelled User");
        context.Customers.Add(customer);

        var act = async () => await context.SaveChangesAsync(cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>(
            because: "SavingChangesAsync must propagate cancellation when CancellationToken is cancelled.");
    }

    [Fact]
    public async Task SavingChangesAsync_WhenCancelledDuringDispatch_PropagatesOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        var dispatcher = Substitute.For<IDomainEventDispatcher>();
        dispatcher.DispatchAsync(Arg.Any<IReadOnlyList<IDomainEvent>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var token = callInfo.Arg<CancellationToken>();
                cts.Cancel();
                token.ThrowIfCancellationRequested();
                return ValueTask.CompletedTask;
            });

        var interceptor = new DomainEventsInterceptor(dispatcher);
        var options = CreateInMemoryOptions();

        await using var context = new TestSharedKernelDbContext(options, interceptor);
        var customer = new CustomerAggregate(CustomerId.New(), "MidFlight Cancelled User");
        context.Customers.Add(customer);

        var act = async () => await context.SaveChangesAsync(cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>(
            because: "SavingChangesAsync must propagate OperationCanceledException when cancelled during event dispatch.");
    }

    [Fact]
    public void SavingChanges_WhenDispatcherThrows_PropagatesException()
    {
        var dispatcher = new ThrowingDispatcher();
        var interceptor = new DomainEventsInterceptor(dispatcher);
        var options = CreateInMemoryOptions();

        using var context = new TestSharedKernelDbContext(options, interceptor);
        var customer = new CustomerAggregate(CustomerId.New(), "Failing Dispatcher User");
        context.Customers.Add(customer);

        var act = () => context.SaveChanges();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Dispatched event handling failed intentionally.",
                because: "Synchronous SavingChanges must propagate any exception thrown by the event dispatcher.");
    }

    [Fact]
    public async Task SavingChangesAsync_WhenDispatcherThrows_PropagatesException()
    {
        var dispatcher = new ThrowingDispatcher();
        var interceptor = new DomainEventsInterceptor(dispatcher);
        var options = CreateInMemoryOptions();

        await using var context = new TestSharedKernelDbContext(options, interceptor);
        var customer = new CustomerAggregate(CustomerId.New(), "Failing Async Dispatcher User");
        context.Customers.Add(customer);

        var act = async () => await context.SaveChangesAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Dispatched event handling failed intentionally.",
                because: "Asynchronous SavingChangesAsync must propagate any exception thrown by the event dispatcher.");
    }

    #endregion
}
