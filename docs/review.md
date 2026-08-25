# Architecture Review & Governance Checklist

---

## 1. Governance Review Checklist

- [x] Zero functional dependencies in core `EricksonLopez.SharedKernel`.
- [x] All persistence adapters segregated into satellite packages (`EntityFrameworkCore`, `Dapper`).
- [x] All types sealed or declared `readonly record struct` where applicable.
- [x] Multi-targeting .NET 8, 9, and 10 with full NativeAOT compatibility.
- [x] 100% English documentation with kebab-case naming.
