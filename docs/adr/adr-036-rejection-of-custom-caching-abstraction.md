# ADR-036: Rejection of Custom Caching Abstraction in Favor of Native Microsoft BCL APIs

## Status
Accepted — August 2026

## Context
During the ecosystem-wide capability ownership audit, the potential introduction of a custom caching abstraction package (`EricksonLopez.Caching.Abstractions` / `EricksonLopez.Caching`) was evaluated.

Modern .NET (.NET 8, 9, 10) provides mature, high-performance, and standardized caching primitives directly within the Base Class Library (BCL) and Microsoft Extensions:
- `Microsoft.Extensions.Caching.Memory.IMemoryCache` (in-process cache).
- `Microsoft.Extensions.Caching.Distributed.IDistributedCache` (distributed cache).
- `Microsoft.Extensions.Caching.Hybrid.HybridCache` (introduced in .NET 9 for unified two-tier L1/L2 caching with stampede protection, tag-based eviction, and Native AOT serialization).

## Decision
Strictly **reject** introducing a custom `EricksonLopez.Caching` abstraction layer.

Ecosystem libraries and consuming applications must directly utilize Microsoft's native abstractions:
1. Applications needing caching must configure and inject standard `HybridCache`, `IDistributedCache`, or `IMemoryCache`.
2. Repositories and query pipelines requiring cache invalidation or retrieval should accept `HybridCache` or `IDistributedCache` as standard constructor dependencies.
3. Provider implementations (such as Redis, Garnet, or in-memory) will continue to rely on standard `Microsoft.Extensions.Caching.StackExchangeRedis` or official Microsoft providers.

## Consequences
- **Positive**: Zero proprietary abstraction overhead; 100% interoperability with the broader .NET open-source ecosystem.
- **Positive**: Immediate benefit from official Microsoft optimizations, Native AOT support, and telemetry in `HybridCache`.
- **Zero Ecosystem Redundancy**: Avoids maintaining wrapper APIs that provide no distinct architectural value over the BCL.
