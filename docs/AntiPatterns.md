# Anti-Patterns

This document lists common mistakes and anti-patterns to avoid when using `EricksonLopez.SharedKernel`.

## 1. Throwing `InvalidOperationException` on `Result.Value`
**Anti-pattern:** Accessing `.Value` without checking `.IsSuccess` first.
```csharp
// BAD
var result = GetUser(id);
Console.WriteLine(result.Value.Name); // Might throw!
```
**Solution:** Use `.Match()`, `.Map()`, or explicit checks.
```csharp
// GOOD
result.Tap(user => Console.WriteLine(user.Name));
```

## 2. Wrapping Every Single Result in Try-Catch
**Anti-pattern:** Using `Result.Try` for simple operations that don't inherently throw exceptions, just to be "safe".
```csharp
// BAD
var result = Result.Try(() => "Hello", ex => Error.Unexpected(...));
```
**Solution:** Only use `Result.Try` when bridging with legacy code or APIs that throw exceptions. For your own domain logic, return `Result.Failure` directly.

## 3. Creating Custom IError Interfaces
**Anti-pattern:** Creating your own error hierarchies or interfaces.
**Solution:** Use the provided `sealed record Error`. It is designed to be unified. If you need more metadata, structure it in the `Code` or `Description`, or nest multiple errors using `InnerErrors`.

## 4. Re-creating Validation Logic Outside the Domain
**Anti-pattern:** Using ASP.NET Core `[Required]` attributes or `FluentValidation` to enforce core domain rules (like "An order must have at least one item").
**Solution:** `FluentValidation` is fine for API input formatting, but core invariant enforcement should happen inside your `AggregateRoot` constructor or factory methods, returning an `Error.Conflict` or `Error.Validation`.

## 5. Awaiting Inside `.Tap` Instead of `.Map`
**Anti-pattern:** Performing side-effects that modify the result or chaining asynchronous operations incorrectly.
```csharp
// BAD - Fire and forget side effect
resultTask.Tap(async user => await _emailService.SendAsync(user));
```
**Solution:** Use the async overloads provided in `ResultExtensions` properly. The library provides `Tap` and `Map` with `Func<T, Task>` overloads that safely await the inner task.
