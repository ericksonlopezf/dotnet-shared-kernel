// Copyright © Erickson Lopez. MIT License.
using System;

namespace Microsoft.Extensions.DependencyInjection;

using System.Diagnostics.CodeAnalysis;
using EricksonLopez.SharedKernel;
using EricksonLopez.SharedKernel.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
/// Provides extension methods for registering SharedKernel Entity Framework Core interceptors and event dispatchers in an <see cref="IServiceCollection"/>.
/// </summary>
public static class SharedKernelEntityFrameworkServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="DomainEventsInterceptor"/> as a scoped <see cref="ISaveChangesInterceptor"/> in the service collection.
    /// </summary>
    /// <param name="services">The service collection to which the interceptor will be added.</param>
    /// <returns>The service collection instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/></exception>
    public static IServiceCollection AddSharedKernelDomainEventsInterceptor(this IServiceCollection services)
    {
        // Stryker disable once Statement: services.AddScoped also throws ArgumentNullException with param "services"
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<ISaveChangesInterceptor, DomainEventsInterceptor>();
        return services;
    }

    /// <summary>
    /// Registers a concrete <see cref="IDomainEventDispatcher"/> implementation and the <see cref="DomainEventsInterceptor"/> as scoped services in the service collection.
    /// </summary>
    /// <typeparam name="TDispatcher">The type of the domain event dispatcher implementation.</typeparam>
    /// <param name="services">The service collection to which the dispatcher and interceptor will be added.</param>
    /// <returns>The service collection instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/></exception>
    public static IServiceCollection AddSharedKernelDomainEventsInterceptor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TDispatcher>(this IServiceCollection services)
        where TDispatcher : class, IDomainEventDispatcher
    {
        // Stryker disable once Statement: services.AddScoped also throws ArgumentNullException with param "services"
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IDomainEventDispatcher, TDispatcher>();
        services.AddScoped<ISaveChangesInterceptor, DomainEventsInterceptor>();
        return services;
    }
}


