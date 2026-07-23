using EricksonLopez.SharedKernel.Pagination;
using AwesomeAssertions;

namespace EricksonLopez.SharedKernel.Tests.Pagination;

public sealed class PagedListTests
{
    [Fact]
    public void Create_ShouldSetMetadataCorrectly()
    {
        var items = Enumerable.Range(1, 10);
        var parameters = PaginationParameters.Of(2, 10);

        var page = PagedList<int>.Create(items, 45, parameters);

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
        var page = PagedList<int>.Create([1, 2, 3], 30, PaginationParameters.Of(1, 3));

        page.HasPreviousPage.Should().BeFalse();
        page.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void LastPage_ShouldNotHaveNextPage()
    {
        var page = PagedList<int>.Create([28, 29, 30], 30, PaginationParameters.Of(10, 3));

        page.HasPreviousPage.Should().BeTrue();
        page.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void Empty_ShouldReturnEmptyPage()
    {
        var page = PagedList<string>.Empty(PaginationParameters.Default);

        page.Items.Should().BeEmpty();
        page.TotalCount.Should().Be(0);
        page.TotalPages.Should().Be(0);
        page.HasPreviousPage.Should().BeFalse();
        page.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void Map_ShouldProjectItemsPreservingMetadata()
    {
        var page = PagedList<int>.Create([1, 2, 3], 30, PaginationParameters.Of(2, 3));
        var mapped = page.Map(x => x.ToString());

        mapped.Items.Should().BeEquivalentTo(["1", "2", "3"]);
        mapped.TotalCount.Should().Be(30);
        mapped.Page.Should().Be(2);
        mapped.PageSize.Should().Be(3);
    }

    [Fact]
    public void PaginationParameters_PageSizeAboveMax_ShouldBeClampedToMax()
    {
        var parameters = PaginationParameters.Of(1, 9999);
        parameters.PageSize.Should().Be(PaginationParameters.MaxPageSize);
    }

    [Fact]
    public void PaginationParameters_Skip_ShouldCalculateCorrectOffset()
    {
        var parameters = PaginationParameters.Of(3, 10);
        parameters.Skip.Should().Be(20); // (3-1) * 10
    }

    [Fact]
    public void TotalPages_WhenPageSizeIsZero_ShouldBeZero()
    {
        var pagedList = Activator.CreateInstance(typeof(PagedList<int>), 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, 
            null, [new List<int>(), 10, 1, 0], null) as PagedList<int>;

        pagedList!.TotalPages.Should().Be(0);
    }

    [Fact]
    public void Create_NullArguments_ShouldThrow()
    {
        var act1 = () => PagedList<int>.Create(null!, 10, PaginationParameters.Default);
        act1.Should().Throw<ArgumentNullException>().WithParameterName("items");

        var act2 = () => PagedList<int>.Create([], 10, null!);
        act2.Should().Throw<ArgumentNullException>().WithParameterName("parameters");
    }
}
