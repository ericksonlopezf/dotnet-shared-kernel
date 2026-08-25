// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Dapper;
using EricksonLopez.DomainPrimitives;

namespace EricksonLopez.SharedKernel.Dapper;

/// <summary>
/// Provides methods to register Dapper type handlers for strongly-typed domain identifiers.
/// </summary>
/// <remarks>
/// Registration should occur once during application startup or composition root configuration.
/// </remarks>
public static class DapperStrongIdRegistry
{
    /// <summary>
    /// Registers a Dapper type handler for the specified strongly-typed identifier.
    /// </summary>
    /// <remarks>
    /// This method is fully compatible with Native AOT and trimming.
    /// </remarks>
    /// <typeparam name="TSelf">The strongly-typed identifier type.</typeparam>
    /// <typeparam name="TValue">The underlying primitive value type.</typeparam>
    public static void Register<TSelf, TValue>()
        where TSelf : notnull, IStrongId<TSelf, TValue>
        where TValue : notnull, IEquatable<TValue>
    {
        SqlMapper.AddTypeHandler<TSelf>(
            new StrongIdTypeHandler<TSelf, TValue>());
    }

    /// <summary>
    /// Scans an assembly and registers Dapper type handlers for all concrete types implementing <see cref="IStrongId{TSelf,TValue}"/>.
    /// </summary>
    /// <param name="assembly">The assembly to scan for strongly-typed identifier types.</param>
    /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/></exception>
    /// <remarks>
    /// <para>
    /// <b>Native AOT / Trimming Incompatible.</b> This method uses reflection-based
    /// assembly scanning (<see cref="System.Reflection.Assembly.GetTypes"/>) and
    /// constructs generic <see cref="StrongIdTypeHandler{TSelf,TValue}"/> instances
    /// via <c>Activator.CreateInstance</c> at runtime. Both operations
    /// are incompatible with Native AOT (<c>IL3050</c>) and Trimming (<c>IL2026</c>).
    /// </para>
    /// <para>
    /// <b>AOT-safe alternative:</b> Register each strongly-typed identifier explicitly
    /// using <see cref="Register{TSelf,TValue}()"/> during application startup:
    /// </para>
    /// <code>
    /// DapperStrongIdRegistry.Register&lt;OrderId, Guid&gt;();
    /// DapperStrongIdRegistry.Register&lt;CustomerId, Guid&gt;();
    /// </code>
    /// <para>
    /// Use this overload only in non-AOT, non-trimmed environments
    /// (e.g., traditional ASP.NET Core without <c>PublishAot=true</c> or
    /// <c>PublishTrimmed=true</c>) and only during application composition startup,
    /// never in hot paths.
    /// </para>
    /// </remarks>
    [RequiresUnreferencedCode("Assembly scanning relies on dynamic reflection which is incompatible with trimming.")]
    [RequiresDynamicCode("Constructing generic TypeHandlers at runtime requires dynamic code generation.")]
    public static void RegisterFromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        IEnumerable<Type> types;

        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            types = ex.Types.OfType<Type>();
        }

        foreach (var type in types)
        {
            if (type.IsAbstract || type.IsInterface)
                continue;

            var strongIdInterface = type
                .GetInterfaces()
                .FirstOrDefault(static iface =>
                    iface.IsGenericType &&
                    iface.GetGenericTypeDefinition() ==
                    typeof(IStrongId<,>));

            if (strongIdInterface is null)
                continue;

            var genericArguments =
                strongIdInterface.GetGenericArguments();

            var handlerType =
                typeof(StrongIdTypeHandler<,>)
                    .MakeGenericType(genericArguments);

            var handler =
                (SqlMapper.ITypeHandler)
                Activator.CreateInstance(handlerType)!;

            SqlMapper.AddTypeHandler(type, handler);
        }
    }

    /// <summary>
    /// Scans multiple assemblies and registers Dapper type handlers for all concrete types implementing <see cref="IStrongId{TSelf,TValue}"/>.
    /// </summary>
    /// <param name="assemblies">An array of assemblies to scan for strongly-typed identifier types.</param>
    /// <exception cref="ArgumentNullException"><paramref name="assemblies"/> is <see langword="null"/></exception>
    /// <remarks>
    /// This method uses reflection and dynamic code generation and is incompatible with Native AOT and Trimming.
    /// </remarks>
    [RequiresUnreferencedCode("Assembly scanning relies on dynamic reflection which is incompatible with trimming.")]
    [RequiresDynamicCode("Constructing generic TypeHandlers at runtime requires dynamic code generation.")]
    public static void RegisterFromAssemblies(params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        foreach (var assembly in assemblies)
        {
            if (assembly is not null)
            {
                RegisterFromAssembly(assembly);
            }
        }
    }
}

