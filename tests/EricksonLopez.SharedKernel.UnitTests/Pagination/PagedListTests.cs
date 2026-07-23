using EricksonLopez.SharedKernel.Pagination;

namespace EricksonLopez.SharedKernel.UnitTests.Pagination;

public sealed class PagedListTests
{
    [Fact]
    public void Create_ShouldSetMetadataCorrectly()
    {
        // Arrange
        var items = Enumerable.Range(1, 10);
        var parameters = PaginationParameters.Of(2, 10);

        // Act
        var page = PagedList<int>.Create(items, 45, parameters);

        // Assert
        page.Page.Should().Be(2);
        page.PageSize.Should().Be(10);
        page.TotalCount.Should().Be(45);
        page.TotalPages.Should().Be(5);
        page.HasPreviousPage.Should().BeTrue();
        page.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void FirstPage_ShouldNotHavePreviousPage()
    {
        // Arrange & Act
        var page = PagedList<int>.Create([1, 2, 3], 30, PaginationParameters.Of(1, 3));

        // Assert
        page.HasPreviousPage.Should().BeFalse();
        page.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void LastPage_ShouldNotHaveNextPage()
    {
        // Arrange & Act
        var page = PagedList<int>.Create([28, 29, 30], 30, PaginationParameters.Of(10, 3));

        // Assert
        page.HasPreviousPage.Should().BeTrue();
        page.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void Empty_ShouldReturnEmptyPage()
    {
        // Arrange & Act
        var page = PagedList<string>.Empty(PaginationParameters.Default);

        // Assert
        page.Items.Should().BeEmpty();
        page.TotalCount.Should().Be(0);
        page.TotalPages.Should().Be(0);
        page.HasPreviousPage.Should().BeFalse();
        page.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void Map_ShouldProjectItemsPreservingMetadata()
    {
        // Arrange
        var page = PagedList<int>.Create([1, 2, 3], 30, PaginationParameters.Of(2, 3));
        
        // Act
        var mapped = page.Map(x => x.ToString());

        // Assert
        mapped.Items.Should().BeEquivalentTo(["1", "2", "3"]);
        mapped.TotalCount.Should().Be(30);
        mapped.Page.Should().Be(2);
        mapped.PageSize.Should().Be(3);
    }

    [Fact]
    public void PaginationParameters_PageSizeAboveMax_ShouldBeClampedToMax()
    {
        // Arrange & Act
        var parameters = PaginationParameters.Of(1, 9999);
        
        // Assert
        parameters.PageSize.Should().Be(PaginationParameters.MaxPageSize);
    }

    [Fact]
    public void PaginationParameters_Skip_ShouldCalculateCorrectOffset()
    {
        // Arrange & Act
        var parameters = PaginationParameters.Of(3, 10);
        
        // Assert
        parameters.Skip.Should().Be(20); // (3-1) * 10
    }

    [Fact]
    public void PaginationParameters_PageBelowOne_ShouldClampToOne()
    {
        // Arrange & Act
        var parameters1 = PaginationParameters.Of(0, 10);
        var parameters2 = PaginationParameters.Of(-5, 10);

        // Assert
        parameters1.Page.Should().Be(1);
        parameters2.Page.Should().Be(1);
    }

    [Fact]
    public void PaginationParameters_PageSizeBelowOne_ShouldClampToOne()
    {
        // Arrange & Act
        var parameters1 = PaginationParameters.Of(1, 0);
        var parameters2 = PaginationParameters.Of(1, -5);

        // Assert
        parameters1.PageSize.Should().Be(1);
        parameters2.PageSize.Should().Be(1);
    }

    [Fact]
    public void PagedList_WithSinglePage_ShouldHaveNoPreviousOrNextPage()
    {
        // Arrange & Act
        var page = PagedList<int>.Create([1, 2], 2, PaginationParameters.Of(1, 10));

        // Assert
        page.HasPreviousPage.Should().BeFalse();
        page.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void PagedList_Items_ShouldBeReadOnly()
    {
        // Arrange
        var page = PagedList<int>.Create([1, 2, 3], 3, PaginationParameters.Default);

        // Act
        var asList = page.Items as List<int>;
        
        // Assert
        asList.Should().BeNull("Items should be exposed as a ReadOnlyCollection to prevent mutation");
        
        var asIList = (IList<int>)page.Items;
        Action act = () => asIList.Add(4);
        
        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void Create_NullArguments_ShouldThrow()
    {
        // Arrange & Act
        var act1 = () => PagedList<int>.Create(null!, 10, PaginationParameters.Default);
        var act2 = () => PagedList<int>.Create([], 10, null!);

        // Assert
        act1.Should().Throw<ArgumentNullException>().WithParameterName("items");
        act2.Should().Throw<ArgumentNullException>().WithParameterName("parameters");
    }

    // ─── Additional boundary tests ────────────────────────────────────────────

    [Fact]
    public void Map_OnEmptyPage_ShouldReturnEmptyMappedPage_WithSameMetadata()
    {
        // Arrange
        var emptyPage = PagedList<int>.Empty(PaginationParameters.Of(3, 10));

        // Act
        var mapped = emptyPage.Map(x => x.ToString());

        // Assert
        mapped.Items.Should().BeEmpty();
        mapped.TotalCount.Should().Be(0);
        mapped.Page.Should().Be(3);
        mapped.PageSize.Should().Be(10);
    }

    [Fact]
    public void PaginationParameters_Default_IsSameInstanceAceseed()
    {
        // Act
        var d1 = PaginationParameters.Default;
        var d2 = PaginationParameters.Default;

        // Assert
        d1.Should().BeSameAs(d2, "Default is a static readonly singleton");
        d1.Page.Should().Be(PaginationParameters.DefaultPage);
        d1.PageSize.Should().Be(PaginationParameters.DefaultPageSize);
    }

    [Fact]
    public void PaginationParameters_Skip_OnFirstPage_ShouldBeZero()
    {
        // Arrange & Act
        var parameters = PaginationParameters.Of(1, 10);

        // Assert
        parameters.Skip.Should().Be(0, "(1 - 1) * 10 = 0");
    }

    [Fact]
    public void PaginationParameters_MaxPageSize_Exact_ShouldNotClamp()
    {
        // Arrange & Act
        var parameters = PaginationParameters.Of(1, PaginationParameters.MaxPageSize);

        // Assert
        parameters.PageSize.Should().Be(PaginationParameters.MaxPageSize,
            "a value exactly at MaxPageSize must not be clamped");
    }

    [Fact]
    public void TotalPages_OddItemCount_ShouldCeilCorrectly()
    {
        // Arrange — 11 items / 10 per page = ceiling(1.1) = 2 pages
        var page = PagedList<int>.Create([1], 11, PaginationParameters.Of(1, 10));

        // Assert
        page.TotalPages.Should().Be(2, "ceiling(11/10) = 2");
    }

    [Fact]
    public void TotalPages_ExactDivision_ShouldNotAddExtraPage()
    {
        // Arrange — 20 items / 10 per page = exactly 2 pages
        var page = PagedList<int>.Create(Enumerable.Range(1, 10), 20, PaginationParameters.Of(1, 10));

        // Assert
        page.TotalPages.Should().Be(2, "20/10 divides evenly — no ceiling needed");
    }

    [Fact]
    public void Empty_NullParameters_ShouldThrow()
    {
        // Act
        var act = () => PagedList<string>.Empty(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("parameters");
    }

    [Fact]
    public void Map_PreservesItemCount_AndTotalCount()
    {
        // Arrange
        var page = PagedList<int>.Create([1, 2, 3, 4, 5], 50, PaginationParameters.Of(2, 5));

        // Act
        var mapped = page.Map(x => x * 10);

        // Assert
        mapped.Items.Should().HaveCount(5);
        mapped.Items.Should().Equal(10, 20, 30, 40, 50);
        mapped.TotalCount.Should().Be(50);
        mapped.TotalPages.Should().Be(10);
    }
}
