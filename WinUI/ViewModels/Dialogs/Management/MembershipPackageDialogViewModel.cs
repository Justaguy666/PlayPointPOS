using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Entities;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using WinUI.UIModels;
using WinUI.UIModels.Enums;

namespace WinUI.ViewModels.Dialogs.Management;

public partial class MembershipPackageDialogViewModel : LocalizedViewModelBase
{
    private const string DefaultRankColor = "#F09A44";

    private readonly IDialogService _dialogService;
    private Func<MembershipRank, Task>? _onMembershipRankAddedAsync;
    private Func<MembershipRank, Task>? _onMembershipRankDeletedAsync;
    private Func<MembershipRank, Task>? _onMembershipRankUpdatedAsync;

    public IconState PaintIconState { get; } = new()
    {
        Kind = IconKind.Paint,
        Size = 16,
        AlwaysFilled = true,
    };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAddMembershipRank))]
    [NotifyCanExecuteChangedFor(nameof(AddMembershipRankCommand))]
    public partial string NewMembershipRankName { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAddMembershipRank))]
    [NotifyCanExecuteChangedFor(nameof(AddMembershipRankCommand))]
    public partial string NewMinSpentText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAddMembershipRank))]
    [NotifyCanExecuteChangedFor(nameof(AddMembershipRankCommand))]
    public partial string NewDiscountText { get; set; } = string.Empty;

    private Color _newMembershipRankColor = ParseColor(DefaultRankColor);
    public Color NewMembershipRankColor
    {
        get => _newMembershipRankColor;
        set
        {
            if (SetProperty(ref _newMembershipRankColor, value))
            {
                NewMembershipRankBrush = new SolidColorBrush(value);
            }
        }
    }

    private Brush _newMembershipRankBrush = new SolidColorBrush(ParseColor(DefaultRankColor));
    public Brush NewMembershipRankBrush
    {
        get => _newMembershipRankBrush;
        set => SetProperty(ref _newMembershipRankBrush, value);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string ErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CloseTooltipText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AddButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NamePlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MinSpentPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DiscountPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MinSpentLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DiscountLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ColorButtonText { get; set; } = string.Empty;

    public ObservableCollection<MembershipPackageItemViewModel> MembershipRanks { get; } = [];

    public event Action? CloseRequested;
    public event Action? DialogHideRequested;
    public event Action? DialogShowRequested;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool CanAddMembershipRank =>
        !string.IsNullOrWhiteSpace(NewMembershipRankName)
        && TryParseOptionalDecimal(NewMinSpentText, out _)
        && TryParseOptionalDecimal(NewDiscountText, out _);

    public MembershipPackageDialogViewModel(ILocalizationService localizationService, IDialogService dialogService)
        : base(localizationService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        RefreshLocalizedText();
    }

    protected override void RefreshLocalizedText()
    {
        Title = LocalizationService.GetString("MembershipPackageDialogTitleText");
        CloseTooltipText = LocalizationService.GetString("CloseTooltipText");
        AddButtonText = LocalizationService.GetString("MembershipPackageDialogAddButtonText");
        NamePlaceholderText = LocalizationService.GetString("MembershipPackageDialogNamePlaceholderText");
        MinSpentPlaceholderText = LocalizationService.GetString("MembershipPackageDialogMinSpentPlaceholderText");
        DiscountPlaceholderText = LocalizationService.GetString("MembershipPackageDialogDiscountPlaceholderText");
        MinSpentLabelText = LocalizationService.GetString("MembershipPackageDialogMinSpentLabelText");
        DiscountLabelText = LocalizationService.GetString("MembershipPackageDialogDiscountLabelText");
        ColorButtonText = LocalizationService.GetString("MembershipPackageDialogColorButtonText");

        foreach (MembershipPackageItemViewModel item in MembershipRanks)
        {
            item.RefreshLocalizedText(LocalizationService);
        }
    }

    public void Configure(MembershipPackageDialogRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        _onMembershipRankAddedAsync = request.OnMembershipRankAddedAsync;
        _onMembershipRankDeletedAsync = request.OnMembershipRankDeletedAsync;
        _onMembershipRankUpdatedAsync = request.OnMembershipRankUpdatedAsync;
        ErrorMessage = string.Empty;

        MembershipRanks.Clear();
        foreach (MembershipRank rank in request.MembershipRanks.OrderBy(rank => rank.MinSpentAmount).ThenBy(rank => rank.Priority))
        {
            MembershipRanks.Add(new MembershipPackageItemViewModel(rank, this, LocalizationService));
        }
    }

    [RelayCommand]
    private void Close()
    {
        CloseRequested?.Invoke();
    }

    [RelayCommand(CanExecute = nameof(CanAddMembershipRank))]
    private async Task AddMembershipRankAsync()
    {
        string trimmedName = NewMembershipRankName.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName)
            || !TryParseOptionalDecimal(NewMinSpentText, out decimal minSpent)
            || !TryParseOptionalDecimal(NewDiscountText, out decimal discountPercent))
        {
            ErrorMessage = LocalizationService.GetString("MembershipPackageDialogInvalidInputText");
            return;
        }

        var newRank = new MembershipRank
        {
            Name = trimmedName,
            MinSpentAmount = minSpent,
            DiscountRate = discountPercent / 100m,
            Color = ToHexColor(NewMembershipRankColor),
        };

        if (_onMembershipRankAddedAsync is not null)
        {
            await _onMembershipRankAddedAsync(newRank);
        }

        MembershipRanks.Add(new MembershipPackageItemViewModel(newRank, this, LocalizationService));
        ReorderItems();

        NewMembershipRankName = string.Empty;
        NewMinSpentText = string.Empty;
        NewDiscountText = string.Empty;
        NewMembershipRankColor = ParseColor(DefaultRankColor);
        ErrorMessage = string.Empty;
    }

    public async Task DeleteMembershipRankAsync(MembershipPackageItemViewModel item)
    {
        DialogHideRequested?.Invoke();

        bool isConfirmed;
        try
        {
            isConfirmed = await _dialogService.ShowConfirmationAsync(
                titleKey: "ConfirmDeleteMembershipPackageTitle",
                messageKey: "ConfirmDeleteMembershipPackageMessage",
                confirmButtonTextKey: "ConfirmDeleteMembershipPackageButton",
                cancelButtonTextKey: "CancelButtonText");
        }
        finally
        {
            DialogShowRequested?.Invoke();
        }

        if (!isConfirmed)
        {
            return;
        }

        if (_onMembershipRankDeletedAsync is not null)
        {
            await _onMembershipRankDeletedAsync(item.MembershipRank);
        }

        MembershipRanks.Remove(item);
        ReorderItems();
    }

    public async Task UpdateMembershipRankAsync(MembershipPackageItemViewModel item, string newName, string newMinSpentText, string newDiscountText, Color newColor)
    {
        if (string.IsNullOrWhiteSpace(newName)
            || !TryParseOptionalDecimal(newMinSpentText, out decimal newMinSpent)
            || !TryParseOptionalDecimal(newDiscountText, out decimal newDiscountPercent))
        {
            return;
        }

        item.MembershipRank.Name = newName.Trim();
        item.MembershipRank.MinSpentAmount = newMinSpent;
        item.MembershipRank.DiscountRate = newDiscountPercent / 100m;
        item.MembershipRank.Color = ToHexColor(newColor);
        item.RefreshDisplay();
        ReorderItems();

        if (_onMembershipRankUpdatedAsync is not null)
        {
            await _onMembershipRankUpdatedAsync(item.MembershipRank);
        }
    }

    public async Task OpenEditMembershipRankDialogAsync(MembershipPackageItemViewModel item)
    {
        ArgumentNullException.ThrowIfNull(item);

        DialogHideRequested?.Invoke();

        try
        {
            await _dialogService.ShowDialogAsync(
                "MembershipPackageEdit",
                new MembershipPackageEditDialogRequest
                {
                    Item = item,
                    OnSubmittedAsync = async (target, name, minSpentText, discountText, color) =>
                    {
                        await UpdateMembershipRankAsync(target, name, minSpentText, discountText, color);
                    },
                });
        }
        finally
        {
            DialogShowRequested?.Invoke();
        }
    }

    internal static string ResolveRankColor(string? rankName, int index)
    {
        string normalizedRankName = rankName?.Trim() ?? string.Empty;

        if (normalizedRankName.Equals("Diamond", StringComparison.OrdinalIgnoreCase))
        {
            return "#9B8BFF";
        }

        if (normalizedRankName.Equals("Platinum", StringComparison.OrdinalIgnoreCase))
        {
            return "#72C5E4";
        }

        if (normalizedRankName.Equals("Gold", StringComparison.OrdinalIgnoreCase))
        {
            return "#F9CC45";
        }

        if (normalizedRankName.Equals("Silver", StringComparison.OrdinalIgnoreCase))
        {
            return "#CBD3DF";
        }

        if (normalizedRankName.Equals("Bronze", StringComparison.OrdinalIgnoreCase))
        {
            return "#F09A44";
        }

        string[] palette =
        [
            "#F09A44",
            "#CBD3DF",
            "#F9CC45",
            "#72C5E4",
            "#9B8BFF",
        ];

        return palette[Math.Abs(index) % palette.Length];
    }

    private void ReorderItems()
    {
        List<MembershipPackageItemViewModel> orderedItems = MembershipRanks
            .OrderBy(item => item.MembershipRank.MinSpentAmount)
            .ThenBy(item => item.MembershipRank.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        MembershipRanks.Clear();

        for (int index = 0; index < orderedItems.Count; index++)
        {
            MembershipPackageItemViewModel item = orderedItems[index];
            item.MembershipRank.Priority = index + 1;
            item.MembershipRank.IsDefault = index == 0;
            MembershipRanks.Add(item);
        }
    }

    private static bool TryParseOptionalDecimal(string? text, out decimal value)
    {
        string trimmedText = text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(trimmedText))
        {
            value = 0m;
            return true;
        }

        bool success = decimal.TryParse(trimmedText, out decimal parsedValue);
        value = success ? Math.Max(0m, parsedValue) : 0m;
        return success;
    }

    internal static Color ParseColor(string? hexColor)
    {
        if (string.IsNullOrWhiteSpace(hexColor))
        {
            return ParseColor(DefaultRankColor);
        }

        string value = hexColor.Trim().TrimStart('#');
        if (value.Length == 6 && uint.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out uint rgb))
        {
            return Color.FromArgb(
                0xFF,
                (byte)((rgb >> 16) & 0xFF),
                (byte)((rgb >> 8) & 0xFF),
                (byte)(rgb & 0xFF));
        }

        if (value.Length == 8 && uint.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out uint argb))
        {
            return Color.FromArgb(
                (byte)((argb >> 24) & 0xFF),
                (byte)((argb >> 16) & 0xFF),
                (byte)((argb >> 8) & 0xFF),
                (byte)(argb & 0xFF));
        }

        return ParseColor(DefaultRankColor);
    }

    internal static string ToHexColor(Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}
