# EricksonLopez.SharedKernel.Dapper.Tests

This project contains adapter tests for Dapper integrations with the Shared Kernel, including type handlers and reflection-based dynamic registries.

## State Isolation Limitations (FIRST Principle - Independent)

Dapper's `SqlMapper` maintains a process-wide static type-handler cache. The static registry (`SqlMapper.AddTypeHandler`) cannot be easily torn down between test executions without resorting to undocumented internal reflection hacks.

Because of this limitation, the tests in this project that mutate the static registry (such as `DapperStrongIdRegistry` tests) must be serialized to avoid race conditions. This is handled by decorating the test classes with xUnit's `[Collection]` attribute (e.g., `[Collection("DapperRegistryTests")]`).

This ensures serialized execution against the static registry while maintaining deterministic and idempotent assertions, fulfilling the FIRST independence principle within the constraints of Dapper's architecture.
