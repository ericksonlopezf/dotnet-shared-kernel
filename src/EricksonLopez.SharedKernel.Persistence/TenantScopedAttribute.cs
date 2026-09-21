// Copyright © Erickson Lopez. MIT License.

namespace EricksonLopez.SharedKernel.Persistence;

using System;

/// <summary>
/// Marks an entity as tenant-scoped. The infrastructure layer will automatically append tenant isolation rules (RLS) to queries for this entity.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class TenantScopedAttribute : Attribute
{
}
