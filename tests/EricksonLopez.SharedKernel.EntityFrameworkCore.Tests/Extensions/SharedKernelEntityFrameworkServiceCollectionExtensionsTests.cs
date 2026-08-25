// Copyright © Erickson Lopez. MIT License.
namespace EricksonLopez.SharedKernel.EntityFrameworkCore.Tests.Extensions;

using System;
using AwesomeAssertions;
using EricksonLopez.SharedKernel.EntityFrameworkCore;
using EricksonLopez.SharedKernel.EntityFrameworkCore.Tests.Fakes;
using EricksonLopez.SharedKernel.TestingUtilities.Fakes;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class SharedKernelEntityFrameworkServiceCollectionExtensionsTests
{
    [Fact]
    public void AddSharedKernelDomainEventsInterceptor_WithNullServices_ThrowsArgumentNullException()
    {
        IServiceCollection services = null!;

        var act1 = () => services.AddSharedKernelDomainEventsInterceptor();
        var act2 = () => services.AddSharedKernelDomainEventsInterceptor<TestDispatcher>();

        act1.Should().Throw<ArgumentNullException>().WithParameterName("services");
        act2.Should().Throw<ArgumentNullException>().WithParameterName("services");
    }

    [Fact]
    public void AddSharedKernelDomainEventsInterceptor_NonGeneric_RegistersInterceptorOnly()
    {
        var services = new ServiceCollection();
        var returnedServices = services.AddSharedKernelDomainEventsInterceptor();
        returnedServices.Should().BeSameAs(services);

        using var provider = services.BuildServiceProvider();

        var dispatcher = provider.GetService<IDomainEventDispatcher>();
        dispatcher.Should().BeNull();

        var interceptor = provider.GetService<ISaveChangesInterceptor>();
        interceptor.Should().NotBeNull();
        interceptor.Should().BeOfType<DomainEventsInterceptor>();
    }

    [Fact]
    public void AddSharedKernelDomainEventsInterceptor_RegistersBothDispatcherAndInterceptor()
    {
        var services = new ServiceCollection();
        var returnedServices = services.AddSharedKernelDomainEventsInterceptor<TestDispatcher>();
        returnedServices.Should().BeSameAs(services);

        using var provider = services.BuildServiceProvider();

        var dispatcher = provider.GetService<IDomainEventDispatcher>();
        dispatcher.Should().NotBeNull();
        dispatcher.Should().BeOfType<TestDispatcher>();

        var interceptor = provider.GetService<ISaveChangesInterceptor>();
        interceptor.Should().NotBeNull();
        interceptor.Should().BeOfType<DomainEventsInterceptor>();
    }
}

