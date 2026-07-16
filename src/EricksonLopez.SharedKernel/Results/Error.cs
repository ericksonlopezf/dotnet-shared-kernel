namespace EricksonLopez.SharedKernel.Results;

/// <summary>
/// Represents a structured error with a code and human-readable description.
/// </summary>
/// <remarks>
/// Error codes should follow the convention: "Domain.ErrorType"
/// Example: "User.NotFound", "Order.InsufficientStock", "Payment.Declined"
///
/// Error types:
/// - <see cref="Failure"/> — Generic domain error (default)
/// - <see cref="Validation"/> — Input validation failure
/// - <see cref="NotFound"/> — Resource not found
/// - <see cref="Conflict"/> — Conflicting state (e.g., duplicate)
/// - <see cref="Unauthorized"/> — Authentication required
/// - <see cref="Forbidden"/> — Insufficient permissions
/// </remarks>
public sealed record Error
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    private Error(string code, string description, ErrorType type)
    {
        Code = code;
        Description = description;
        Type = type;
    }

    public string Code { get; }
    public string Description { get; }
    public ErrorType Type { get; }

    // ─── Factory methods ──────────────────────────────────────────────────────

    public static Error Failure(string code, string description)
        => new(code, description, ErrorType.Failure);

    public static Error Validation(string code, string description)
        => new(code, description, ErrorType.Validation);

    public static Error NotFound(string code, string description)
        => new(code, description, ErrorType.NotFound);

    public static Error Conflict(string code, string description)
        => new(code, description, ErrorType.Conflict);

    public static Error Unauthorized(string code, string description)
        => new(code, description, ErrorType.Unauthorized);

    public static Error Forbidden(string code, string description)
        => new(code, description, ErrorType.Forbidden);

    public override string ToString() => $"[{Type}] {Code}: {Description}";
}

public enum ErrorType
{
    Failure,
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden
}
