namespace Application.UseCases.Pagination.Contracts;

public sealed record BuildPaginationStateResult(
    int CurrentPage,
    int TotalItems,
    int PageSize,
    int TotalPages,
    int MaxVisiblePages,
    IReadOnlyList<int> VisiblePages,
    bool CanMoveToFirstPage,
    bool CanMoveToPreviousPage,
    bool CanMoveToNextPage,
    bool CanMoveToLastPage);
