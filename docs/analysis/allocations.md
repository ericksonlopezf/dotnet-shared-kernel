# Allocation & Memory Profile Analysis

This document details the memory footprint, benchmark disassembly, and NativeAOT execution guarantees of `EricksonLopez.SharedKernel`.

---

## 1. Zero-Allocation Domain Invariants

| Operation | Standard DDD Framework | `EricksonLopez.SharedKernel` | Improvement |
|---|---|---|---|
| Entity ID Allocation | 24 B (`class` or boxed Guid) | **0 B** (`readonly record struct`) | **100% Zero Allocation** |
| Domain Event Collection | Heap `List<IDomainEvent>` on ctor | **0 B** (Lazy initialized backing list) | **100% Zero Alloc on Read-only entities** |
| Dapper UNNEST Parameter Binding | Array allocations | **0 B** (Span/Stack-allocated array buffers) | **100% Zero Allocation** |
