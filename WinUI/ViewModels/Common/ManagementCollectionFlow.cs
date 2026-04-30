using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using WinUI.UIModels;

namespace WinUI.ViewModels.Common;

public static class ManagementCollectionFlow
{
    public static bool IsPaginationProperty(string? propertyName)
    {
        return propertyName is nameof(PaginationModel.CurrentPage)
            or nameof(PaginationModel.PageSize)
            or nameof(PaginationModel.TotalItems);
    }

    public static void ApplyPaginationUpdate(
        PaginationModel pagination,
        int totalItems,
        bool resetToFirstPage,
        Action refreshPage)
    {
        ArgumentNullException.ThrowIfNull(pagination);
        ArgumentNullException.ThrowIfNull(refreshPage);

        pagination.TotalItems = totalItems;

        if (resetToFirstPage && pagination.CurrentPage != 1)
        {
            pagination.CurrentPage = 1;
            return;
        }

        int totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)Math.Max(pagination.PageSize, 1)));
        if (pagination.CurrentPage > totalPages)
        {
            pagination.CurrentPage = totalPages;
            return;
        }

        refreshPage();
    }

    public static PageInfoSnapshot BuildPageInfo(PaginationModel pagination, int totalItems)
    {
        ArgumentNullException.ThrowIfNull(pagination);

        int normalizedPageSize = Math.Max(pagination.PageSize, 1);
        int startItem = totalItems == 0 ? 0 : ((pagination.CurrentPage - 1) * normalizedPageSize) + 1;
        int endItem = totalItems == 0 ? 0 : Math.Min(pagination.CurrentPage * normalizedPageSize, totalItems);
        int totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)normalizedPageSize));

        return new PageInfoSnapshot(
            startItem,
            endItem,
            totalItems,
            pagination.CurrentPage,
            totalPages);
    }

    public static bool ContainsSearchText(CultureInfo culture, string? source, string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return true;
        }

        return culture.CompareInfo.IndexOf(source ?? string.Empty, searchText, CompareOptions.IgnoreCase) >= 0;
    }

    public static void ReplaceOptions<T>(ObservableCollection<T> collection, IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(items);

        collection.Clear();
        foreach (T item in items)
        {
            collection.Add(item);
        }
    }
}

public readonly record struct PageInfoSnapshot(
    int StartItem,
    int EndItem,
    int TotalItems,
    int CurrentPage,
    int TotalPages);
