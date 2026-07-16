namespace EricksonLopez.SharedKernel.Pagination;

/// <summary>
/// Parameters for requesting a paginated result set.
/// </summary>
/// <remarks>
/// Page numbers are 1-indexed.
/// PageSize is clamped between 1 and <see cref="MaxPageSize"/> to prevent
/// abusive queries.
/// </remarks>
public sealed class PaginationParameters
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;

    private int _pageSize = DefaultPageSize;

    public int Page { get; init; } = DefaultPage;

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = Math.Clamp(value, 1, MaxPageSize);
    }

    /// <summary>
    /// Calculates the number of items to skip for SQL OFFSET.
    /// </summary>
    public int Skip => (Page - 1) * PageSize;

    /// <summary>
    /// Creates default pagination parameters (page 1, 10 items per page).
    /// </summary>
    public static PaginationParameters Default => new();

    /// <summary>
    /// Creates pagination parameters with explicit values.
    /// </summary>
    public static PaginationParameters Of(int page, int pageSize) => new()
    {
        Page = Math.Max(1, page),
        PageSize = pageSize
    };
}
