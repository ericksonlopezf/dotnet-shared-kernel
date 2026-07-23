using System;
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

    private int _page = DefaultPage;

    public int Page
    {
        get => _page;
        init => _page = Math.Max(1, value);
    }

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = Math.Max(1, Math.Min(MaxPageSize, value));
    }

    /// <summary>
    /// Calculates the number of items to skip for SQL OFFSET.
    /// </summary>
    public int Skip => (Page - 1) * PageSize;

    /// <summary>
    /// Creates default pagination parameters (page 1, 10 items per page).
    /// </summary>
    public static readonly PaginationParameters Default = new();

    /// <summary>
    /// Creates pagination parameters with explicit values.
    /// </summary>
    public static PaginationParameters Of(int page, int pageSize) => new()
    {
        Page = page,
        PageSize = pageSize
    };
}

