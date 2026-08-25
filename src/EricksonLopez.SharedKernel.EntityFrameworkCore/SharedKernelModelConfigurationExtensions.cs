// Copyright © Erickson Lopez. MIT License.
using System;

namespace Microsoft.EntityFrameworkCore;

using EricksonLopez.DomainPrimitives;
using EricksonLopez.SharedKernel;
using EricksonLopez.SharedKernel.EntityFrameworkCore;

/// <summary>
/// Provides extension methods for configuring strongly-typed identifiers and domain events on <see cref="ModelConfigurationBuilder"/> and <see cref="ModelBuilder"/>.
/// </summary>
public static class SharedKernelModelConfigurationExtensions
{
    /// <summary>
    /// Configures a <see cref="StrongIdValueConverter{TId, TValue}"/> for the specified strongly-typed identifier type.
    /// </summary>
    /// <typeparam name="TId">The strongly-typed identifier type.</typeparam>
    /// <typeparam name="TValue">The underlying primitive value type.</typeparam>
    /// <param name="configurationBuilder">The model configuration builder.</param>
    /// <returns>The modified model configuration builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configurationBuilder"/> is <see langword="null"/></exception>
    public static ModelConfigurationBuilder ConfigureStrongId<TId, TValue>(this ModelConfigurationBuilder configurationBuilder)
        where TId : notnull, IStrongId<TId, TValue>
        where TValue : notnull, IEquatable<TValue>
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);

        configurationBuilder.Properties<TId>().HaveConversion<StrongIdValueConverter<TId, TValue>>();
        return configurationBuilder;
    }

    /// <summary>
    /// Scans the specified assembly and configures value converters for all concrete types implementing <see cref="IStrongId{TSelf,TValue}"/>.
    /// </summary>
    /// <param name="configurationBuilder">The model configuration builder.</param>
    /// <param name="assembly">The assembly to scan for strongly-typed identifier types.</param>
    /// <returns>The modified model configuration builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configurationBuilder"/> or <paramref name="assembly"/> is <see langword="null"/></exception>
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("Assembly scanning relies on dynamic reflection which is incompatible with trimming.")]
    [System.Diagnostics.CodeAnalysis.RequiresDynamicCode("Constructing generic ValueConverters at runtime requires dynamic code generation.")]
    public static ModelConfigurationBuilder ConfigureStrongIdsFromAssembly(
        this ModelConfigurationBuilder configurationBuilder,
        System.Reflection.Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);
        ArgumentNullException.ThrowIfNull(assembly);

        System.Collections.Generic.IEnumerable<Type> types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (System.Reflection.ReflectionTypeLoadException ex)
        {
            types = System.Linq.Enumerable.OfType<Type>(ex.Types);
        }

        foreach (var type in types)
        {
            if (type.IsAbstract || type.IsInterface)
                continue;

            var strongIdInterface = System.Linq.Enumerable.FirstOrDefault(
                type.GetInterfaces(),
                static iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IStrongId<,>));

            if (strongIdInterface is null)
                continue;

            var genericArguments = strongIdInterface.GetGenericArguments();
            var converterType = typeof(StrongIdValueConverter<,>).MakeGenericType(genericArguments);

            configurationBuilder.Properties(type).HaveConversion(converterType);
        }

        return configurationBuilder;
    }

    /// <summary>
    /// Scans multiple assemblies and configures value converters for all concrete types implementing <see cref="IStrongId{TSelf,TValue}"/>.
    /// </summary>
    /// <param name="configurationBuilder">The model configuration builder.</param>
    /// <param name="assemblies">An array of assemblies to scan for strongly-typed identifier types.</param>
    /// <returns>The modified model configuration builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configurationBuilder"/> or <paramref name="assemblies"/> is <see langword="null"/></exception>
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("Assembly scanning relies on dynamic reflection which is incompatible with trimming.")]
    [System.Diagnostics.CodeAnalysis.RequiresDynamicCode("Constructing generic ValueConverters at runtime requires dynamic code generation.")]
    public static ModelConfigurationBuilder ConfigureStrongIdsFromAssemblies(
        this ModelConfigurationBuilder configurationBuilder,
        params System.Reflection.Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);
        ArgumentNullException.ThrowIfNull(assemblies);

        foreach (var assembly in assemblies)
        {
            if (assembly is not null)
            {
                configurationBuilder.ConfigureStrongIdsFromAssembly(assembly);
            }
        }

        return configurationBuilder;
    }

    /// <summary>
    /// Configures the model builder to ignore domain event draining methods across all entity types implementing <see cref="IHasDomainEvents"/>.
    /// </summary>
    /// <param name="modelBuilder">The model builder to configure.</param>
    /// <returns>The modified model builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="modelBuilder"/> is <see langword="null"/></exception>
    /// <remarks>
    /// <para>
    /// The built-in <see cref="EricksonLopez.SharedKernel.AggregateRoot{TId}"/> stores
    /// domain events in a <c>private</c> field. <see cref="IHasDomainEvents.DrainDomainEvents"/>
    /// is a method — not a property — so EF Core does not map it by default.
    /// </para>
    /// <para>
    /// This call is a <b>defensive convention</b>: it explicitly registers the member
    /// name as ignored in the model metadata, guarding against future EF Core behavior
    /// changes or custom aggregate subclasses that might inadvertently expose domain
    /// event state as a mappable property.
    /// </para>
    /// <para>
    /// If your aggregate subclass adds a <b>public property</b> of type
    /// <c>IReadOnlyList&lt;IDomainEvent&gt;</c> or similar, configure it explicitly per entity:
    /// </para>
    /// <code>
    /// modelBuilder.Entity&lt;Order&gt;().Ignore(o =&gt; o.DomainEvents);
    /// </code>
    /// <para>
    /// When using <see cref="DomainEventsInterceptor"/>, no additional configuration is
    /// required — the interceptor drains events via <see cref="IHasDomainEvents.DrainDomainEvents"/>
    /// which is already not a mappable property.
    /// </para>
    /// </remarks>
    public static ModelBuilder IgnoreDomainEvents(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IHasDomainEvents).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).Ignore(nameof(IHasDomainEvents.DrainDomainEvents));
            }
        }

        return modelBuilder;
    }
}




