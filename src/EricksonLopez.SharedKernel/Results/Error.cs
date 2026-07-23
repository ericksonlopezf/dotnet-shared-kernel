namespace EricksonLopez.SharedKernel.Results;

/// <summary>
/// Represents a structured, immutable domain error.
/// </summary>
/// <remarks>
/// <para>
/// Error codes should follow the convention: "Domain.ErrorType".
/// Example: "User.NotFound", "Order.InsufficientStock", "Payment.Declined"
/// </para>
/// <para>
/// For compound errors (e.g., multiple validation failures), use factory overloads
/// that accept inner errors:
/// <code>
/// var error = Error.Validation("User.Invalid", "Validation failed",
///     Error.Validation("User.Name.Required", "Name is required"),
///     Error.Validation("User.Email.Invalid", "Invalid email format"));
/// </code>
/// </para>
/// <para>
/// <b>Equality:</b> Two errors are equal if they share the same
/// <see cref="Code"/>, <see cref="Description"/>, <see cref="Type"/>,
/// and <see cref="InnerErrors"/>. This follows standard record semantics
/// where all fields participate in equality.
/// </para>
/// </remarks>
public sealed record Error
{
    /// <summary>
    /// Sentinel value representing the absence of an error. Used internally
    /// by <see cref="Result"/> to represent the success state.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    private readonly IReadOnlyList<Error>? _innerErrors;

    private Error(string code, string description, ErrorType type, IReadOnlyList<Error>? innerErrors = null)
    {
        Code = code;
        Description = description;
        Type = type;
        _innerErrors = innerErrors is { Count: > 0 } ? innerErrors : null;
    }

    /// <summary>Unique, machine-readable error identifier (e.g., "User.NotFound").</summary>
    public string Code { get; }

    /// <summary>Human-readable error description.</summary>
    public string Description { get; }

    /// <summary>Semantic categorization of the error.</summary>
    public ErrorType Type { get; }

    /// <summary>
    /// Child errors for compound error scenarios (e.g., multiple validation failures).
    /// Returns an empty list when there are no inner errors.
    /// </summary>
    /// <remarks>
    /// This is a computed property and does NOT participate in record equality.
    /// The backing field is null for simple errors (zero allocation).
    /// </remarks>
    public IReadOnlyList<Error> InnerErrors => _innerErrors ?? Array.Empty<Error>();

    /// <summary>
    /// Whether this error contains inner errors.
    /// </summary>
    public bool HasInnerErrors => _innerErrors is { Count: > 0 };

    // ─── Factory methods ──────────────────────────────────────────────────────

    /// <summary>Creates a generic domain error.</summary>
    public static Error Failure(string code, string description)
        => new(code, description, ErrorType.Failure);

    /// <summary>Creates a generic domain error with inner errors.</summary>
    public static Error Failure(string code, string description, params Error[] innerErrors)
        => new(code, description, ErrorType.Failure, innerErrors);

    /// <summary>Creates an input validation error.</summary>
    public static Error Validation(string code, string description)
        => new(code, description, ErrorType.Validation);

    /// <summary>Creates a validation error with field-level inner errors.</summary>
    public static Error Validation(string code, string description, params Error[] innerErrors)
        => new(code, description, ErrorType.Validation, innerErrors);

    /// <summary>Creates a resource-not-found error.</summary>
    public static Error NotFound(string code, string description)
        => new(code, description, ErrorType.NotFound);

    /// <summary>Creates a not-found error with inner errors.</summary>
    public static Error NotFound(string code, string description, params Error[] innerErrors)
        => new(code, description, ErrorType.NotFound, innerErrors);

    /// <summary>Creates a state-conflict error (duplicate, concurrent modification).</summary>
    public static Error Conflict(string code, string description)
        => new(code, description, ErrorType.Conflict);

    /// <summary>Creates a conflict error with inner errors.</summary>
    public static Error Conflict(string code, string description, params Error[] innerErrors)
        => new(code, description, ErrorType.Conflict, innerErrors);

    /// <summary>Creates an authentication-required error.</summary>
    public static Error Unauthorized(string code, string description)
        => new(code, description, ErrorType.Unauthorized);

    /// <summary>Creates an unauthorized error with inner errors.</summary>
    public static Error Unauthorized(string code, string description, params Error[] innerErrors)
        => new(code, description, ErrorType.Unauthorized, innerErrors);

    /// <summary>Creates an insufficient-permissions error.</summary>
    public static Error Forbidden(string code, string description)
        => new(code, description, ErrorType.Forbidden);

    /// <summary>Creates a forbidden error with inner errors.</summary>
    public static Error Forbidden(string code, string description, params Error[] innerErrors)
        => new(code, description, ErrorType.Forbidden, innerErrors);

    /// <summary>Creates a service/resource temporarily unavailable error.</summary>
    public static Error Unavailable(string code, string description)
        => new(code, description, ErrorType.Unavailable);

    /// <summary>Creates an unavailable error with inner errors.</summary>
    public static Error Unavailable(string code, string description, params Error[] innerErrors)
        => new(code, description, ErrorType.Unavailable, innerErrors);

    /// <summary>Creates an unexpected system error (wrapped exceptions, invariant violations).</summary>
    public static Error Unexpected(string code, string description)
        => new(code, description, ErrorType.Unexpected);

    /// <summary>Creates an unexpected error with inner errors.</summary>
    public static Error Unexpected(string code, string description, params Error[] innerErrors)
        => new(code, description, ErrorType.Unexpected, innerErrors);

    public override string ToString()
    {
        var result = $"[{Type}] {Code}: {Description}";
        if (HasInnerErrors)
            result += $" ({InnerErrors.Count} inner errors)";
        return result;
    }
}

/// <summary>
/// Semantic categorization for <see cref="Error"/>.
/// Each type has clear domain semantics without coupling to HTTP or any transport.
/// </summary>
public enum ErrorType
{
    /// <summary>Generic domain error (default).</summary>
    Failure,

    /// <summary>Input or data validation failure.</summary>
    Validation,

    /// <summary>Requested resource does not exist.</summary>
    NotFound,

    /// <summary>State conflict (duplicate, concurrent modification).</summary>
    Conflict,

    /// <summary>Authentication required.</summary>
    Unauthorized,

    /// <summary>Insufficient permissions (authenticated but not allowed).</summary>
    Forbidden,

    /// <summary>Service or resource temporarily unavailable.</summary>
    Unavailable,

    /// <summary>Unexpected system error (wrapped exceptions, invariant violations).</summary>
    Unexpected
}
