using System;
namespace EricksonLopez.SharedKernel.Pagination;

/// <summary>
/// A paginated list of items with metadata about the full result set.
/// </summary>
/// <typeparam name="T">The type of items in the list.</typeparam>
/// <remarks>
/// Use <see cref="PagedList{T}.Create"/> to construct from a list and total count,
/// or <see cref="PagedList{T}.Empty"/> for an empty page.
///
/// Example (with Dapper):
/// <code>
/// var items = await connection.QueryAsync&lt;Product&gt;(sql, new { offset, limit });
/// var total = await connection.ExecuteScalarAsync&lt;int&gt;(countSql);
/// return PagedList&lt;Product&gt;.Create(items, total, parameters);
/// </code>
/// </remarks>
public sealed class PagedList<T>
{
    private PagedList(
        IEnumerable<T> items,
        int totalCount,
        int page,
        int pageSize)
    {
        Items = items.ToList().AsReadOnly();
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }

    /// <summary>The items on the current page.</summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>Total number of items across all pages.</summary>
    public int TotalCount { get; }

    /// <summary>The current page number (1-indexed).</summary>
    public int Page { get; }

    /// <summary>The number of items per page.</summary>
    public int PageSize { get; }

    /// <summary>Total number of pages.</summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>Whether there is a page before the current one.</summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>Whether there is a page after the current one.</summary>
    public bool HasNextPage => Page < TotalPages;

    // ─── Factory methods ──────────────────────────────────────────────────────

    /// <summary>
    /// Creates a <see cref="PagedList{T}"/> from an already-paged collection and total count.
    /// </summary>
    /// <param name="items">The items on the current page (already sliced by the data source).</param>
    /// <param name="totalCount">The total number of matching items in the data source.</param>
    /// <param name="parameters">The pagination parameters used for the query.</param>
    public static PagedList<T> Create(
        IEnumerable<T> items,
        int totalCount,
        PaginationParameters parameters)
    {
        if (items is null) throw new ArgumentNullException(nameof(items));
        if (parameters is null) throw new ArgumentNullException(nameof(parameters));

        return new PagedList<T>(items, totalCount, parameters.Page, parameters.PageSize);
    }

    /// <summary>
    /// Creates a <see cref="PagedList{T}"/> with no items.
    /// </summary>
    public static PagedList<T> Empty(PaginationParameters parameters)
    {
        if (parameters is null) throw new ArgumentNullException(nameof(parameters));
        return new([], 0, parameters.Page, parameters.PageSize);
    }

    /// <summary>
    /// Projects each item to a new type, preserving pagination metadata.
    /// </summary>
    public PagedList<TResult> Map<TResult>(Func<T, TResult> selector)
        => new(Items.Select(selector), TotalCount, Page, PageSize);
}

