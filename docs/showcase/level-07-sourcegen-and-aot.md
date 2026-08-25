# Level 07 — NativeAOT Source Generators & Serialization

In Level 07, we explore Roslyn source generators for domain primitives and NativeAOT serialization using `EricksonLopez.SharedKernel.SourceGenerators` and `EricksonLopez.SharedKernel.Json`.

---

## 1. Source Generated Strongly-Typed IDs

```csharp
using EricksonLopez.SharedKernel;

[StronglyTypedId]
public readonly partial struct CustomerId;
```

The generator automatically emits JSON converters, EF Core ValueConverters, and Dapper TypeHandlers at compile time.
