using Application.UseCases.Pagination.Contracts;

namespace Application.UseCases.Pagination;

public sealed class BuildPaginationStateUseCase
{
    public BuildPaginationStateResult Execute(BuildPaginationStateRequest request)
    {
        int totalItems = Math.Max(request.TotalItems, 0);
        int pageSize = Math.Max(request.PageSize, 1);
        int maxVisiblePages = Math.Max(request.MaxVisiblePages, 1);
        int totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
        int currentPage = Math.Clamp(request.CurrentPage, 1, totalPages);

        int visiblePageCount = Math.Min(totalPages, maxVisiblePages);
        int maxStartPage = Math.Max(1, totalPages - visiblePageCount + 1);
        int startPage = Math.Clamp(currentPage - ((visiblePageCount - 1) / 2), 1, maxStartPage);
        int endPage = startPage + visiblePageCount - 1;

        int[] visiblePages = Enumerable
            .Range(startPage, endPage - startPage + 1)
            .ToArray();

        bool canMoveBackward = currentPage > 1;
        bool canMoveForward = currentPage < totalPages;

        return new BuildPaginationStateResult(
            currentPage,
            totalItems,
            pageSize,
            totalPages,
            maxVisiblePages,
            visiblePages,
            canMoveBackward,
            canMoveBackward,
            canMoveForward,
            canMoveForward);
    }
}
