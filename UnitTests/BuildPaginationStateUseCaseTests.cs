using Application.UseCases.Pagination;
using Application.UseCases.Pagination.Contracts;

namespace UnitTests;

public class BuildPaginationStateUseCaseTests
{
    private readonly BuildPaginationStateUseCase _useCase = new();

    [Fact]
    public void Execute_ReturnsFirstWindow_WhenCurrentPageIsNearStart()
    {
        var result = _useCase.Execute(new BuildPaginationStateRequest(
            CurrentPage: 1,
            TotalItems: 95,
            PageSize: 10,
            MaxVisiblePages: 4));

        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(10, result.TotalPages);
        Assert.Equal([1, 2, 3, 4], result.VisiblePages);
        Assert.False(result.CanMoveToFirstPage);
        Assert.False(result.CanMoveToPreviousPage);
        Assert.True(result.CanMoveToNextPage);
        Assert.True(result.CanMoveToLastPage);
    }

    [Fact]
    public void Execute_ReturnsSlidingWindow_WhenCurrentPageIsInMiddle()
    {
        var result = _useCase.Execute(new BuildPaginationStateRequest(
            CurrentPage: 5,
            TotalItems: 95,
            PageSize: 10,
            MaxVisiblePages: 4));

        Assert.Equal(5, result.CurrentPage);
        Assert.Equal([4, 5, 6, 7], result.VisiblePages);
    }

    [Fact]
    public void Execute_ReturnsLastWindow_WhenCurrentPageIsNearEnd()
    {
        var result = _useCase.Execute(new BuildPaginationStateRequest(
            CurrentPage: 10,
            TotalItems: 95,
            PageSize: 10,
            MaxVisiblePages: 4));

        Assert.Equal(10, result.CurrentPage);
        Assert.Equal([7, 8, 9, 10], result.VisiblePages);
        Assert.True(result.CanMoveToFirstPage);
        Assert.True(result.CanMoveToPreviousPage);
        Assert.False(result.CanMoveToNextPage);
        Assert.False(result.CanMoveToLastPage);
    }

    [Fact]
    public void Execute_ClampsInvalidInput_AndNormalizesPaginationSettings()
    {
        var result = _useCase.Execute(new BuildPaginationStateRequest(
            CurrentPage: 99,
            TotalItems: -5,
            PageSize: 0,
            MaxVisiblePages: 0));

        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(0, result.TotalItems);
        Assert.Equal(1, result.PageSize);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(1, result.MaxVisiblePages);
        Assert.Equal([1], result.VisiblePages);
    }
}
