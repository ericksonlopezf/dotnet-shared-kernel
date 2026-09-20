# ADR-037: Persistence Metadata Submodule Architecture

## Status
Accepted (Supersedes standalone `dotnet-persistence` repository)

## Date
2026-09-04

## Context
The `EricksonLopez` ecosystem heavily relies on Dapper and PostgreSQL for high-performance data access. Traditionally, Dapper maps POCOs to database columns either via implicit naming conventions or via run-time reflection attributes (like `[Column("...")]`).
In a Native AOT environment (`IsAotCompatible=true`, `IsTrimmable=true`), run-time reflection is strictly forbidden or severely degraded. 
Additionally, for MultiTenancy (Row-Level Security) and Auditing (`CreatedAt`, `UpdatedAt`), the infrastructure layer needs to know which tables support `TenantId` and which properties are audit fields, without resorting to base classes (to prefer composition) or reflection.

Previously, an isolated repository `dotnet-persistence` attempted to define these abstractions, but remained disconnected, unmaintained, and lacked a testing harness.

## Decision
1. **Segregated Submodule**: Integrate the persistence metadata primitives directly within `dotnet-shared-kernel` as the submodule **`EricksonLopez.SharedKernel.Persistence`** (preserving the pure DDD nature of the root `EricksonLopez.SharedKernel` package):
   - `[Table(name, schema)]`
   - `[Column(name, isPrimaryKey: bool, isDatabaseGenerated: bool)]`
   - `[TenantScoped]` (flags entities subject to tenant isolation / RLS)
   - `[Audit(AuditFieldType)]` (flags properties holding automated audit data)
   - `EntityMetadata` and `PropertyMetadata` (immutable schema descriptors with pre-indexed column dictionaries).
2. **Compile-Time Source Generation**: Incorporate `MetadataSourceGenerator` into `EricksonLopez.SharedKernel.SourceGenerators`. The generator parses `[Table]` and entity hierarchies at compile time and emits static, zero-reflection `GeneratedMetadataRegistry` descriptors that infrastructure layers query in $O(1)$ time without reflection.
3. **Safety Compilation Guard**: The generator verifies that the target compilation explicitly references `EricksonLopez.SharedKernel.Persistence` before emitting registry code, ensuring that projects consuming only core DDD types remain unimpacted.
4. **Deprecation**: Deprecate and remove the standalone `dotnet-persistence` repository.

## Consequences
- **Positive**: 100% Native AOT compliance for Dapper mapping and Infrastructure metadata queries. Zero run-time reflection overhead. Clean architectural alignment under `dotnet-shared-kernel`.
- **Negative**: Requires entities opting into explicit table mappings to be annotated.
