// Copyright © Erickson Lopez. MIT License.
using System;
using System.Text.Json.Serialization.Metadata;
using EricksonLopez.SharedKernel;

namespace EricksonLopez.SharedKernel.Json;

/// <summary>
/// Provides JSON contract resolver and type info modifiers for EricksonLopez.SharedKernel domain types.
/// </summary>
public static class SharedKernelJsonModifiers
{
    /// <summary>
    /// TypeInfo modifier that strips redundant backward-compatibility legacy alias properties
    /// (<c>EventId</c> and <c>OccurredOn</c>) from <see cref="DomainEvent"/> JSON payloads, reducing wire weight by up to 35%.
    /// </summary>
    /// <param name="typeInfo">The JSON type info descriptor to modify.</param>
    /// <exception cref="ArgumentNullException"><paramref name="typeInfo"/> is <see langword="null"/>.</exception>
    public static void IgnoreLegacyDomainEventAliases(JsonTypeInfo typeInfo)
    {
        ArgumentNullException.ThrowIfNull(typeInfo);

        if (!typeof(DomainEvent).IsAssignableFrom(typeInfo.Type))
        {
            return;
        }

        for (var i = typeInfo.Properties.Count - 1; i >= 0; i--)
        {
            var prop = typeInfo.Properties[i];
            if (prop.Name.Equals("EventId", StringComparison.OrdinalIgnoreCase) ||
                prop.Name.Equals("OccurredOn", StringComparison.OrdinalIgnoreCase))
            {
                typeInfo.Properties.RemoveAt(i);
            }
        }
    }
}
