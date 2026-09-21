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
using Microsoft.EntityFrameworkCore.Diagnostics;
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
    public void SavingChanges_Synchronous_PersistsAndDispatchesEvents()
    {
        // Note: DomainEventsInterceptor.SavedChanges runs synchronously and invokes IDomainEventDispatcher.Dispatch.
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

        dispatcher.Received(1).Dispatch(
            Arg.Is<IReadOnlyList<IDomainEvent>>(events =>
                events.Count == 1 &&
                events.OfType<CustomerRegisteredEvent>().Any(cre => cre.CustomerId == customerId)));
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

        dispatcher.DidNotReceive().Dispatch(
            Arg.Any<IReadOnlyList<IDomainEvent>>());
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

    #region Forensic Audit Remediations (FND-SK-001 & FND-SK-002)

    [Fact]
    public void CollectEvents_ReadOnlyInspection_DoesNotDrainEventsFromTrackedEntities()
    {
        var options = CreateInMemoryOptions();
        using var context = new TestSharedKernelDbContext(options);
        var customer = new CustomerAggregate(CustomerId.New(), "Read Only Event User");
        context.Customers.Add(customer);

        customer.PendingDomainEventsCount.Should().Be(1);

        // Act: CollectEvents inspection
        var collected = DomainEventsInterceptor.CollectEvents(context);

        // Assert: Events are read, but NOT drained
        collected.Should().HaveCount(1);
        customer.PendingDomainEventsCount.Should().Be(1, because: "CollectEvents must provide a non-destructive read-only view per FND-SK-001.");
        customer.DomainEvents.Should().HaveCount(1);

        // Subsequent call still returns the event
        var collectedAgain = DomainEventsInterceptor.CollectEvents(context);
        collectedAgain.Should().HaveCount(1);
    }

    [Fact]
    public void BeforeCommit_WhenDispatchFails_RestoresEventsToAggregates()
    {
        var dispatcher = new ThrowingDispatcher();
        var interceptor = new DomainEventsInterceptor(dispatcher, DomainEventDispatchTiming.BeforeCommit);
        var options = CreateInMemoryOptions();

        using var context = new TestSharedKernelDbContext(options, interceptor);
        var customer = new CustomerAggregate(CustomerId.New(), "Rollback User");
        context.Customers.Add(customer);

        customer.PendingDomainEventsCount.Should().Be(1);

        // Act: context.SaveChanges will fail because ThrowingDispatcher throws in BeforeCommit
        Action act = () => context.SaveChanges();
        act.Should().Throw<InvalidOperationException>();

        // Assert: Events were restored to customer so retry logic or error recovery still has the events!
        customer.PendingDomainEventsCount.Should().Be(1, because: "Failed BeforeCommit dispatch must restore drained events back to entities per FND-SK-002.");
        customer.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public async Task BeforeCommitAsync_WhenDispatchFails_RestoresEventsToAggregates()
    {
        var dispatcher = new ThrowingDispatcher();
        var interceptor = new DomainEventsInterceptor(dispatcher, DomainEventDispatchTiming.BeforeCommit);
        var options = CreateInMemoryOptions();

        await using var context = new TestSharedKernelDbContext(options, interceptor);
        var customer = new CustomerAggregate(CustomerId.New(), "Rollback Async User");
        context.Customers.Add(customer);

        customer.PendingDomainEventsCount.Should().Be(1);

        // Act: context.SaveChangesAsync will fail because ThrowingDispatcher throws in BeforeCommit
        Func<Task> act = async () => await context.SaveChangesAsync();
        await act.Should().ThrowAsync<InvalidOperationException>();

        // Assert: Events were restored to customer so retry logic or error recovery still has the events!
        customer.PendingDomainEventsCount.Should().Be(1, because: "Failed BeforeCommit async dispatch must restore drained events back to entities per FND-SK-002.");
        customer.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Constructor_Parameterless_InitializesWithDefaults()
    {
        var interceptor = new DomainEventsInterceptor();
        interceptor.Should().NotBeNull();
    }

    [Fact]
    public void BeforeCommit_WhenSuccessful_DispatchesSynchronouslyAndDrainsBuffer()
    {
        var dispatcher = Substitute.For<IDomainEventDispatcher>();
        var interceptor = new DomainEventsInterceptor(dispatcher, DomainEventDispatchTiming.BeforeCommit);
        var options = CreateInMemoryOptions();

        using (var context = new TestSharedKernelDbContext(options, interceptor))
        {
            var customer = new CustomerAggregate(CustomerId.New(), "BeforeCommit Sync User");
            context.Customers.Add(customer);
            context.SaveChanges();
            customer.PendingDomainEventsCount.Should().Be(0);
        }

        dispatcher.Received(1).Dispatch(Arg.Any<IReadOnlyList<IDomainEvent>>());
    }

    [Fact]
    public async Task BeforeCommitAsync_WhenSuccessful_DispatchesAsynchronouslyAndDrainsBuffer()
    {
        var dispatcher = Substitute.For<IDomainEventDispatcher>();
        dispatcher.DispatchAsync(Arg.Any<IReadOnlyList<IDomainEvent>>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.CompletedTask);
        var interceptor = new DomainEventsInterceptor(dispatcher, DomainEventDispatchTiming.BeforeCommit);
        var options = CreateInMemoryOptions();

        await using (var context = new TestSharedKernelDbContext(options, interceptor))
        {
            var customer = new CustomerAggregate(CustomerId.New(), "BeforeCommit Async User");
            context.Customers.Add(customer);
            await context.SaveChangesAsync();
            customer.PendingDomainEventsCount.Should().Be(0);
        }

        await dispatcher.Received(1).DispatchAsync(Arg.Any<IReadOnlyList<IDomainEvent>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void AfterCommit_WithOnDispatchError_Sync_InvokesCallbackInsteadOfThrowing()
    {
        Exception? capturedException = null;
        IReadOnlyList<IDomainEvent>? capturedEvents = null;

        var dispatcher = new ThrowingDispatcher();
        var interceptor = new DomainEventsInterceptor(
            dispatcher,
            DomainEventDispatchTiming.AfterCommit,
            (ex, events) =>
            {
                capturedException = ex;
                capturedEvents = events;
            });

        var options = CreateInMemoryOptions();
        using var context = new TestSharedKernelDbContext(options, interceptor);
        var customer = new CustomerAggregate(CustomerId.New(), "Error Callback User");
        context.Customers.Add(customer);

        var act = () => context.SaveChanges();
        act.Should().NotThrow();

        capturedException.Should().NotBeNull();
        capturedEvents.Should().NotBeNull().And.HaveCount(1);
    }

    [Fact]
    public async Task AfterCommitAsync_WithOnDispatchError_Async_InvokesCallbackInsteadOfThrowing()
    {
        Exception? capturedException = null;
        IReadOnlyList<IDomainEvent>? capturedEvents = null;

        var dispatcher = new ThrowingDispatcher();
        var interceptor = new DomainEventsInterceptor(
            dispatcher,
            DomainEventDispatchTiming.AfterCommit,
            (ex, events) =>
            {
                capturedException = ex;
                capturedEvents = events;
            });

        var options = CreateInMemoryOptions();
        await using var context = new TestSharedKernelDbContext(options, interceptor);
        var customer = new CustomerAggregate(CustomerId.New(), "Error Callback Async User");
        context.Customers.Add(customer);

        Func<Task> act = async () => await context.SaveChangesAsync();
        await act.Should().NotThrowAsync();

        capturedException.Should().NotBeNull();
        capturedEvents.Should().NotBeNull().And.HaveCount(1);
    }

    [Fact]
    public async Task Interceptor_NullEventDataGuards_ThrowArgumentNullException()
    {
        var interceptor = new DomainEventsInterceptor();

        var act1 = () => interceptor.SavedChanges(null!, 1);
        act1.Should().Throw<ArgumentNullException>().WithParameterName("eventData");

        var act2 = async () => await interceptor.SavedChangesAsync(null!, 1);
        await act2.Should().ThrowAsync<ArgumentNullException>().WithParameterName("eventData");

        var act3 = () => interceptor.SaveChangesFailed(null!);
        act3.Should().Throw<ArgumentNullException>().WithParameterName("eventData");

        var act4 = async () => await interceptor.SaveChangesFailedAsync(null!);
        await act4.Should().ThrowAsync<ArgumentNullException>().WithParameterName("eventData");
    }

    [Fact]
    public void AfterCommit_WhenEntityHasNoEvents_DoesNotInvokeDispatcher()
    {
        var dispatcher = Substitute.For<IDomainEventDispatcher>();
        var interceptor = new DomainEventsInterceptor(dispatcher, DomainEventDispatchTiming.AfterCommit);
        var options = CreateInMemoryOptions();

        using var context = new TestSharedKernelDbContext(options, interceptor);
        var customer = new CustomerAggregate(CustomerId.New(), "No Events AfterCommit User");
        customer.DrainDomainEvents();
        context.Customers.Add(customer);

        context.SaveChanges();

        dispatcher.DidNotReceive().Dispatch(Arg.Any<IReadOnlyList<IDomainEvent>>());
    }

    [Fact]
    public async Task AfterCommitAsync_WhenEntityHasNoEvents_DoesNotInvokeDispatcher()
    {
        var dispatcher = Substitute.For<IDomainEventDispatcher>();
        var interceptor = new DomainEventsInterceptor(dispatcher, DomainEventDispatchTiming.AfterCommit);
        var options = CreateInMemoryOptions();

        await using var context = new TestSharedKernelDbContext(options, interceptor);
        var customer = new CustomerAggregate(CustomerId.New(), "No Events AfterCommit Async User");
        customer.DrainDomainEvents();
        context.Customers.Add(customer);

        await context.SaveChangesAsync();

        await dispatcher.DidNotReceive().DispatchAsync(Arg.Any<IReadOnlyList<IDomainEvent>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void SaveChangesFailed_WithBeforeCommitDrainedEvents_RestoresEventsToAggregates()
    {
        var dispatcher = Substitute.For<IDomainEventDispatcher>();
        var interceptor = new DomainEventsInterceptor(dispatcher, DomainEventDispatchTiming.BeforeCommit);
        var options = CreateInMemoryOptions();

        using var context = new TestSharedKernelDbContext(options, interceptor);
        var customer = new CustomerAggregate(CustomerId.New(), "Failed DB User");
        context.Customers.Add(customer);

        var savingEventData = (DbContextEventData)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(DbContextEventData));
        var contextField = typeof(DbContextEventData).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).First(f => typeof(DbContext).IsAssignableFrom(f.FieldType));
        contextField.SetValue(savingEventData, context);

        interceptor.SavingChanges(savingEventData, new Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<int>());
        customer.PendingDomainEventsCount.Should().Be(0);

        var errorData = (DbContextErrorEventData)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(DbContextErrorEventData));
        contextField.SetValue(errorData, context);

        interceptor.SaveChangesFailed(errorData);

        customer.PendingDomainEventsCount.Should().Be(1);
    }

    [Fact]
    public async Task SaveChangesFailedAsync_WithBeforeCommitDrainedEvents_RestoresEventsToAggregates()
    {
        var dispatcher = Substitute.For<IDomainEventDispatcher>();
        dispatcher.DispatchAsync(Arg.Any<IReadOnlyList<IDomainEvent>>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.CompletedTask);
        var interceptor = new DomainEventsInterceptor(dispatcher, DomainEventDispatchTiming.BeforeCommit);
        var options = CreateInMemoryOptions();

        await using var context = new TestSharedKernelDbContext(options, interceptor);
        var customer = new CustomerAggregate(CustomerId.New(), "Failed Async DB User");
        context.Customers.Add(customer);

        var savingEventData = (DbContextEventData)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(DbContextEventData));
        var contextField = typeof(DbContextEventData).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).First(f => typeof(DbContext).IsAssignableFrom(f.FieldType));
        contextField.SetValue(savingEventData, context);

        await interceptor.SavingChangesAsync(savingEventData, new Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<int>());
        customer.PendingDomainEventsCount.Should().Be(0);

        var errorData = (DbContextErrorEventData)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(DbContextErrorEventData));
        contextField.SetValue(errorData, context);

        await interceptor.SaveChangesFailedAsync(errorData);

        customer.PendingDomainEventsCount.Should().Be(1);
    }

    #endregion
}

