using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Application.Services;
using Application.Services.Games;
using Application.Services.Products;
using Microsoft.UI.Xaml.Controls;
using WinUI.UIModels.Management;

namespace WinUI.Services;

public sealed class SessionSalePickerService
{
    private sealed class PickTag
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public decimal UnitPrice { get; init; }
        public int StockQuantity { get; init; }
    }

    private readonly WinUIDialogService _dialogs;
    private readonly ILocalizationService _localization;
    private readonly IGameCatalogService _games;
    private readonly IProductCatalogService _products;

    public SessionSalePickerService(
        IDialogService dialogService,
        ILocalizationService localization,
        IGameCatalogService games,
        IProductCatalogService products)
    {
        _dialogs = dialogService as WinUIDialogService
            ?? throw new ArgumentException("Dialog service must be WinUIDialogService.", nameof(dialogService));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _games = games ?? throw new ArgumentNullException(nameof(games));
        _products = products ?? throw new ArgumentNullException(nameof(products));
    }

    public async Task<PendingSessionSaleLine?> PickGameAsync()
    {
        ComboBox combo = BuildGameCombo();
        if (combo.Items.Count == 0)
        {
            return null;
        }

        combo.SelectedIndex = 0;

        var dialog = new ContentDialog
        {
            Title = _localization.GetString("SessionSalePickGameTitle"),
            PrimaryButtonText = _localization.GetString("SessionSalePickerConfirm"),
            CloseButtonText = _localization.GetString("CancelButtonText"),
            DefaultButton = ContentDialogButton.Primary,
            Content = combo,
            XamlRoot = _dialogs.TryGetXamlRoot(),
        };

        if (dialog.XamlRoot is null)
        {
            return null;
        }

        if (await dialog.ShowAsync() != ContentDialogResult.Primary)
        {
            return null;
        }

        if (combo.SelectedItem is not ComboBoxItem selected || selected.Tag is not PickTag tag)
        {
            return null;
        }

        return new PendingSessionSaleLine
        {
            IsGame = true,
            CatalogId = tag.Id,
            DisplayName = tag.Name,
            UnitPrice = tag.UnitPrice,
            GameRentalStartUtc = DateTime.UtcNow,
            ProductQuantity = 0,
        };
    }

    public async Task<PendingSessionSaleLine?> PickProductAsync()
    {
        ComboBox combo = BuildProductCombo();
        if (combo.Items.Count == 0)
        {
            return null;
        }

        combo.SelectedIndex = 0;

        var quantity = new NumberBox
        {
            Minimum = 1,
            Maximum = 999,
            Value = 1,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline,
        };

        var panel = new StackPanel { Spacing = 12 };
        panel.Children.Add(combo);
        panel.Children.Add(new TextBlock { Text = _localization.GetString("SessionSaleQuantityLabel") });
        panel.Children.Add(quantity);

        var dialog = new ContentDialog
        {
            Title = _localization.GetString("SessionSalePickProductTitle"),
            PrimaryButtonText = _localization.GetString("SessionSalePickerConfirm"),
            CloseButtonText = _localization.GetString("CancelButtonText"),
            DefaultButton = ContentDialogButton.Primary,
            Content = panel,
            XamlRoot = _dialogs.TryGetXamlRoot(),
        };

        if (dialog.XamlRoot is null)
        {
            return null;
        }

        if (await dialog.ShowAsync() != ContentDialogResult.Primary)
        {
            return null;
        }

        if (combo.SelectedItem is not ComboBoxItem selected || selected.Tag is not PickTag tag)
        {
            return null;
        }

        int qty = (int)Math.Clamp(Math.Round(quantity.Value), 1, 999);
        qty = Math.Min(qty, Math.Max(1, tag.StockQuantity));

        return new PendingSessionSaleLine
        {
            IsGame = false,
            CatalogId = tag.Id,
            DisplayName = tag.Name,
            UnitPrice = tag.UnitPrice,
            ProductQuantity = qty,
            GameRentalStartUtc = null,
        };
    }

    private ComboBox BuildGameCombo()
    {
        var combo = new ComboBox { MinWidth = 280 };
        foreach (var record in _games.GetGames())
        {
            int id = TryParseId(record.Id);
            if (id <= 0 || record.StockQuantity <= 0)
            {
                continue;
            }

            combo.Items.Add(new ComboBoxItem
            {
                Content = record.Name,
                Tag = new PickTag
                {
                    Id = id,
                    Name = record.Name,
                    UnitPrice = record.HourlyPrice,
                    StockQuantity = record.StockQuantity,
                },
            });
        }

        return combo;
    }

    private ComboBox BuildProductCombo()
    {
        var combo = new ComboBox { MinWidth = 280 };
        foreach (var record in _products.GetProducts())
        {
            int id = TryParseId(record.Id);
            if (id <= 0 || record.StockQuantity <= 0)
            {
                continue;
            }

            combo.Items.Add(new ComboBoxItem
            {
                Content = record.Name,
                Tag = new PickTag
                {
                    Id = id,
                    Name = record.Name,
                    UnitPrice = record.Price,
                    StockQuantity = record.StockQuantity,
                },
            });
        }

        return combo;
    }

    private static int TryParseId(string? id)
    {
        return int.TryParse(id?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) ? n : 0;
    }
}
