# Cookbook

This cookbook contains practical examples and common patterns for using `EricksonLopez.SharedKernel`.

## Returning Validation Errors

Instead of creating a custom error class, use the unified `Error.Validation` method, nesting specific field errors in the `InnerErrors` parameter.

```csharp
public Result<User> CreateUser(string email, string password)
{
    var errors = new List<Error>();

    if (string.IsNullOrWhiteSpace(email))
        errors.Add(Error.Validation("User.EmailRequired", "Email is required."));
    else if (!email.Contains("@"))
        errors.Add(Error.Validation("User.EmailInvalid", "Email format is invalid."));

    if (password.Length < 8)
        errors.Add(Error.Validation("User.PasswordTooShort", "Password must be at least 8 characters."));

    if (errors.Any())
    {
        // Return a single validation error containing all field-level failures
        return Error.Validation(
            "User.ValidationFailed", 
            "The user request failed validation.", 
            errors.ToArray());
    }

    return new User(email, password);
}
```

## Creating the Controller Endpoint (ASP.NET Core)

You can easily map the `Result` pattern to HTTP responses using `.Match()`.

```csharp
[HttpPost]
public async Task<IActionResult> CreateUser(CreateUserRequest request)
{
    var result = await _userService.CreateUserAsync(request.Email, request.Password);

    return result.Match(
        user => Ok(user),
        error => error.Type switch 
        {
            ErrorType.Validation => BadRequest(error),
            ErrorType.Conflict => Conflict(error),
            _ => StatusCode(500, error)
        }
    );
}
```

## Chaining Operations (Fluent API)

The Result extensions allow you to chain operations asynchronously without wrapping everything in `try/catch` or `if/else` blocks.

```csharp
public Task<Result<UserDto>> ProcessUserAsync(Guid id)
{
    return _repository.GetByIdAsync(id) // Returns Task<Result<User>>
        .Ensure(user => user.IsActive, Error.Forbidden("User.Inactive", "User is disabled."))
        .Map(user => _mapper.ToDto(user)) // Maps Result<User> to Result<UserDto>
        .Tap(dto => _logger.LogInformation("Processed user {Id}", dto.Id)) // Side effect
        .TapError(error => _logger.LogWarning("Failed: {Code}", error.Code));
}
```

## Using the Specification Pattern

Combine specifications for query reuse. The `Specification` base class provides NativeAOT-safe `IsSatisfiedBy` evaluation and LINQ-to-SQL compatible `ToExpression()`.

```csharp
public sealed class ActiveUserSpec : Specification<User>
{
    public override Expression<Func<User, bool>> ToExpression()
        => u => u.IsActive;
}

public sealed class AdminUserSpec : Specification<User>
{
    public override Expression<Func<User, bool>> ToExpression()
        => u => u.Role == "Admin";
}

// Usage:
var activeAdminSpec = new ActiveUserSpec().And(new AdminUserSpec());

// In Memory:
var isValid = activeAdminSpec.IsSatisfiedBy(user);

// Database / EF Core:
var users = await dbContext.Users
    .Where(activeAdminSpec.ToExpression())
    .ToListAsync();
```
