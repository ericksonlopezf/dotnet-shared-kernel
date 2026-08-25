# ADR-028: Result-First Architecture over Domain Exceptions

**Date:** 2026-08-15  
**Status:** Accepted  
**Deciders:** Erickson Lopez  
**Backlog Reference:** BL-010  
**Alias:** ADR-0010  

---

## Context

Traditional object-oriented designs frequently utilize custom exceptions (e.g., `DomainException`, `EntityNotFoundException`, `BusinessRuleValidationException`) to signal rule violations and interrupt control flow.

Using exceptions for domain business logic failures introduces significant architectural and operational drawbacks:

1. **Hidden Control Flow:** Exceptions break method contracts by hiding failure modes from caller signatures. Developers cannot determine whether a domain method can fail simply by inspecting its public API.
2. **Performance Overhead:** Instantiating exceptions forces the .NET CLR to capture stack frames and unwind execution stacks. Under high-throughput workloads (e.g., batch processing, electronic invoicing), exception allocation creates CPU spikes and memory pressure.
3. **Imprecise Semantic Mapping:** Domain exceptions blur the boundary between expected business rejections (e.g., "Insufficient funds", "Duplicate invoice number") and unrecoverable exceptional failures (e.g., database network timeouts, hardware failure, null reference bugs).

## Decision

We standardize on a **Result-first (Railway-Oriented Programming)** paradigm across the entire Domain and Application layers. We explicitly prohibit the creation and usage of a `DomainException` base class for business logic and validation flows:

1. **Domain Operations:** All entity methods, domain services, value object factories, and command handlers that can fail due to business rules must return `Result` or `Result<T>`.
2. **Error Representation:** Failures must be represented as strongly typed, immutable `Error` records containing distinct error codes, descriptive messages, and error classifications (e.g., `Validation`, `Conflict`, `NotFound`, `Unauthorized`).
3. **Exceptions Policy:** Native .NET exceptions are strictly reserved for unrecoverable infrastructure, system, or software bug scenarios (e.g., database connection loss, out-of-memory, invalid framework configuration).
4. **Presentation Translation:** The Presentation layer (Web API) intercepts `Result.Failure` and translates `Error` metadata deterministically into RFC 9457 compliant `ProblemDetails` responses via centralized endpoint extensions.

```csharp
// Standard Result & Error Contract Definition
public readonly record struct Error(string Code, string Description, ErrorType Type)
{
    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);
    public static Error Validation(string code, string description) =>
        new(code, description, ErrorType.Validation);
    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);
    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);
}

public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3
}

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
            throw new InvalidOperationException("Invalid Result state");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of a failure result");

    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error) => _value = value;
}
```

```csharp
// Presentation Layer Mapping to RFC 9457 ProblemDetails
public static class ResultExtensions
{
    public static IResult ToProblemDetails(this Result result)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("Cannot convert successful result to problem details");

        return Results.Problem(
            statusCode: result.Error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            },
            title: result.Error.Type.ToString(),
            detail: result.Error.Description,
            extensions: new Dictionary<string, object?>
            {
                { "errorCode", result.Error.Code }
            });
    }
}
```

## Consequences

### Positive
- **Explicit Method Signatures:** APIs clearly document return types and potential failure paths (`Task<Result<InvoiceResponse>>`).
- **Zero Stack Unwinding Overhead:** Expected business rejections do not allocate stack traces, preserving execution speed and predictable latency.
- **Deterministic API Error Mapping:** Centralized mapping from `ErrorType` to standard HTTP status codes without complex middleware exception-catching hierarchies.
- **Clean Code Flow:** Promotes monadic composition (`Map`, `Bind`, `Match`) and clean functional pipelines across Application use cases.

### Negative / Trade-offs
- **Unwrapping Discipline:** Callers must explicitly check `IsSuccess` or bind through railway patterns rather than relying on deep exception bubbling.
- **Language Ergonomics:** Requires consistent adherence to the Result pattern across all team members, reinforced by code review standards and architecture tests.

## Architectural Decision Comparison Matrix

| Quality Attribute | Traditional Exception Pattern | Result-First Pattern (ADR-028 / ADR-0010) |
|---|---|---|
| **API Determinism** | Low (Implicit / Hidden throws) | High (Explicit method signatures) |
| **Throughput & Latency** | Degrades under high error rates | Optimal (No stack trace allocation) |
| **Error Traceability** | High overhead stack inspection | Structured via `Error` domain records |
| **HTTP RFC 9457 Mapping** | Middleware catch blocks | Direct functional projection |
| **Domain Purity** | Coupled to .NET Exception hierarchy | Pure C# domain types |
