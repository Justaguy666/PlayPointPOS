using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WinUI.UIModels;

namespace WinUI.ViewModels.Common;

public sealed class ManagementCollectionController<TItem, TCard> : IDisposable
    where TItem : notnull
{
    private readonly IList<TItem> _items;
    private readonly PaginationModel _pagination;
    private readonly Action _clearPagedCards;
    private readonly Action<TCard> _addPagedCard;
    private readonly Func<IEnumerable<TItem>, IReadOnlyList<TItem>> _queryItems;
    private readonly Func<TItem, TCard> _cardFactory;
    private readonly Func<string>? _cacheScopeProvider;
    private readonly Dictionary<ManagementCardCacheKey<TItem>, TCard> _cardCache = [];
    private bool _isDisposed;

    public ManagementCollectionController(
        IList<TItem> items,
        PaginationModel pagination,
        ObservableCollection<TCard> pagedCards,
        Func<IEnumerable<TItem>, IReadOnlyList<TItem>> queryItems,
        Func<TItem, TCard> cardFactory,
        Func<string>? cacheScopeProvider = null)
        : this(
            items,
            pagination,
            pagedCards.Clear,
            pagedCards.Add,
            queryItems,
            cardFactory,
            cacheScopeProvider)
    {
    }

    public ManagementCollectionController(
        IList<TItem> items,
        PaginationModel pagination,
        Action clearPagedCards,
        Action<TCard> addPagedCard,
        Func<IEnumerable<TItem>, IReadOnlyList<TItem>> queryItems,
        Func<TItem, TCard> cardFactory,
        Func<string>? cacheScopeProvider = null)
    {
        _items = items ?? throw new ArgumentNullException(nameof(items));
        _pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
        _clearPagedCards = clearPagedCards ?? throw new ArgumentNullException(nameof(clearPagedCards));
        _addPagedCard = addPagedCard ?? throw new ArgumentNullException(nameof(addPagedCard));
        _queryItems = queryItems ?? throw new ArgumentNullException(nameof(queryItems));
        _cardFactory = cardFactory ?? throw new ArgumentNullException(nameof(cardFactory));
        _cacheScopeProvider = cacheScopeProvider;
        _pagination.PropertyChanged += HandlePaginationPropertyChanged;
    }

    public IReadOnlyList<TItem> FilteredItems { get; private set; } = [];

    public bool HasItems => FilteredItems.Count > 0;

    public event Action? Refreshed;

    public bool Contains(TItem item)
    {
        return _items.Contains(item);
    }

    public void Insert(int index, TItem item)
    {
        _items.Insert(Math.Clamp(index, 0, _items.Count), item);
    }

    public bool Remove(TItem item)
    {
        bool removed = _items.Remove(item);
        RemoveCachedCards(item);
        return removed;
    }

    public void Refresh(bool resetToFirstPage)
    {
        FilteredItems = _queryItems(_items);
        ManagementCollectionFlow.ApplyPaginationUpdate(
            _pagination,
            FilteredItems.Count,
            resetToFirstPage,
            RefreshPage);
    }

    public void RefreshPage()
    {
        _clearPagedCards();

        int startIndex = Math.Max(0, (_pagination.CurrentPage - 1) * _pagination.PageSize);
        foreach (TItem item in FilteredItems.Skip(startIndex).Take(_pagination.PageSize))
        {
            _addPagedCard(GetOrCreateCard(item));
        }

        Refreshed?.Invoke();
    }

    public void SetPageSize(int newPageSize)
    {
        if (_pagination.PageSize == newPageSize)
        {
            return;
        }

        _pagination.PageSize = Math.Max(1, newPageSize);
        int totalPages = Math.Max(1, (int)Math.Ceiling(FilteredItems.Count / (double)_pagination.PageSize));
        if (_pagination.CurrentPage > totalPages)
        {
            _pagination.CurrentPage = totalPages;
        }
    }

    public PageInfoSnapshot BuildPageInfo()
    {
        return ManagementCollectionFlow.BuildPageInfo(_pagination, FilteredItems.Count);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _pagination.PropertyChanged -= HandlePaginationPropertyChanged;
        foreach (TCard card in _cardCache.Values)
        {
            if (card is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        _cardCache.Clear();
        _isDisposed = true;
    }

    private void HandlePaginationPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (ManagementCollectionFlow.IsPaginationProperty(e.PropertyName))
        {
            RefreshPage();
        }
    }

    private TCard GetOrCreateCard(TItem item)
    {
        var key = new ManagementCardCacheKey<TItem>(item, _cacheScopeProvider?.Invoke() ?? string.Empty);
        if (!_cardCache.TryGetValue(key, out TCard? card))
        {
            card = _cardFactory(item);
            _cardCache[key] = card;
        }

        return card;
    }

    private void RemoveCachedCards(TItem item)
    {
        var keys = _cardCache.Keys
            .Where(key => EqualityComparer<TItem>.Default.Equals(key.Item, item))
            .ToList();

        foreach (var key in keys)
        {
            TCard card = _cardCache[key];
            _cardCache.Remove(key);
            if (card is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    private readonly record struct ManagementCardCacheKey<TCachedItem>(TCachedItem Item, string Scope)
        where TCachedItem : notnull;
}
