using System;
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

public partial class MembershipPackageItemViewModel : ObservableObject
{
    private readonly MembershipPackageDialogViewModel _parent;
    private readonly ILocalizationService _localizationService;

    private const string DefaultColor = "#F09A44";

    public MembershipRank MembershipRank { get; }

    public IconState EditIconState { get; } = new()
    {
        Kind = IconKind.Update,
        Size = 16,
        AlwaysFilled = true,
    };

    public IconState DeleteIconState { get; } = new()
    {
        Kind = IconKind.Delete,
        Size = 16,
        AlwaysFilled = true,
    };

    public IconState PaintIconState { get; } = new()
    {
        Kind = IconKind.Paint,
        Size = 16,
        AlwaysFilled = true,
    };

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial string MinSpentText { get; set; }

    [ObservableProperty]
    public partial string DiscountText { get; set; }

    [ObservableProperty]
    public partial bool IsEditing { get; set; }

    [ObservableProperty]
    public partial string EditName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EditMinSpentText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EditDiscountText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MinSpentLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DiscountLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string RankColorText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ColorButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EditTooltipText { get; set; } = string.Empty;

    private Color _editColor;
    public Color EditColor
    {
        get => _editColor;
        set
        {
            if (SetProperty(ref _editColor, value))
            {
                EditColorBrush = new SolidColorBrush(value);
            }
        }
    }

    private Brush _rankColorBrush = new SolidColorBrush(MembershipPackageDialogViewModel.ParseColor(DefaultColor));
    public Brush RankColorBrush
    {
        get => _rankColorBrush;
        set => SetProperty(ref _rankColorBrush, value);
    }

    private Brush _editColorBrush = new SolidColorBrush(MembershipPackageDialogViewModel.ParseColor(DefaultColor));
    public Brush EditColorBrush
    {
        get => _editColorBrush;
        set => SetProperty(ref _editColorBrush, value);
    }

    public MembershipPackageItemViewModel(
        MembershipRank membershipRank,
        MembershipPackageDialogViewModel parent,
        ILocalizationService localizationService)
    {
        MembershipRank = membershipRank ?? throw new ArgumentNullException(nameof(membershipRank));
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        Name = membershipRank.Name ?? string.Empty;
        EditColor = MembershipPackageDialogViewModel.ParseColor(membershipRank.Color);
        MinSpentText = localizationService.FormatCurrency(membershipRank.MinSpentAmount);
        DiscountText = string.Format(localizationService.Culture, "{0:0.##}%", membershipRank.DiscountRate * 100m);
        RefreshLocalizedText(localizationService);
    }

    public void RefreshLocalizedText(ILocalizationService localizationService)
    {
        MinSpentLabelText = localizationService.GetString("MembershipPackageDialogMinSpentLabelText");
        DiscountLabelText = localizationService.GetString("MembershipPackageDialogDiscountLabelText");
        ColorButtonText = localizationService.GetString("MembershipPackageDialogColorButtonText");
        EditTooltipText = localizationService.GetString("EditButtonText");
        RefreshDisplay(localizationService);
    }

    public void RefreshDisplay()
    {
        RefreshDisplay(_localizationService);
    }

    [RelayCommand]
    private async Task BeginEditAsync()
    {
        EditName = Name;
        EditMinSpentText = MembershipRank.MinSpentAmount.ToString("0.##", _localizationService.Culture);
        EditDiscountText = (MembershipRank.DiscountRate * 100m).ToString("0.##", _localizationService.Culture);
        EditColor = MembershipPackageDialogViewModel.ParseColor(MembershipRank.Color);
        await _parent.OpenEditMembershipRankDialogAsync(this);
    }

    [RelayCommand]
    private async Task CommitEditAsync()
    {
        if (!IsEditing)
        {
            return;
        }

        await _parent.UpdateMembershipRankAsync(this, EditName, EditMinSpentText, EditDiscountText, EditColor);
        IsEditing = false;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
    }

    [RelayCommand]
    private Task DeleteAsync()
    {
        return _parent.DeleteMembershipRankAsync(this);
    }

    private void RefreshDisplay(ILocalizationService localizationService)
    {
        Name = MembershipRank.Name ?? string.Empty;
        MinSpentText = localizationService.FormatCurrency(MembershipRank.MinSpentAmount);
        DiscountText = string.Format(localizationService.Culture, "{0:0.##}%", MembershipRank.DiscountRate * 100m);
        Color parsedColor = MembershipPackageDialogViewModel.ParseColor(MembershipRank.Color);
        RankColorBrush = new SolidColorBrush(parsedColor);
        RankColorText = MembershipPackageDialogViewModel.ToHexColor(parsedColor);
    }
}
