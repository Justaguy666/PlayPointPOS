namespace Application.UseCases.Pagination.Contracts;

public sealed record BuildPaginationStateRequest(
    int CurrentPage,
    int TotalItems,
    int PageSize,
    int MaxVisiblePages = 4);
