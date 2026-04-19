using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading.Tasks;
using Windows.UI;
using WinUI.Resources;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.UIModels.Management;

namespace WinUI.ViewModels.UserControls.Members;

public partial class MemberCardControlViewModel : LocalizedViewModelBase
{
    private readonly Func<MemberModel, Task>? _editAction;
    private readonly Func<MemberModel, Task>? _deleteAction;
    private readonly Brush _defaultWaterBrush;
    private readonly Brush _defaultBubbleBrush;
    private readonly Brush _defaultProgressBrush;
    private bool _isDisposed;

    public MemberCardControlViewModel(
        ILocalizationService localizationService,
        MemberModel model,
        Func<MemberModel, Task>? editAction,
        Func<MemberModel, Task>? deleteAction)
        : base(localizationService)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
        _editAction = editAction;
        _deleteAction = deleteAction;
        _defaultWaterBrush = AppResourceLookup.GetBrush("WarningAmberBrush", AppColors.WarningAmber);
        _defaultBubbleBrush = new SolidColorBrush(Color.FromArgb(90, 255, 255, 255));
        _defaultProgressBrush = AppResourceLookup.GetBrush("PrimaryOrangeBrush", AppColors.PrimaryOrange);

        EditCommand = new AsyncRelayCommand(ExecuteEditAsync);
        DeleteCommand = new AsyncRelayCommand(ExecuteDeleteAsync);

        Model.PropertyChanged += HandleModelPropertyChanged;
        RefreshLocalizedText();
    }

    public MemberModel Model { get; }

    [ObservableProperty]
    public partial string TotalSpentLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MemberIdLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TotalSpentText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ProgressPercentageText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial Brush PackageBadgeBackground { get; set; } = new SolidColorBrush(AppColors.WarningAmber);

    [ObservableProperty]
    public partial Brush PackageBadgeForeground { get; set; } = new SolidColorBrush(AppColors.Black);

    [ObservableProperty]
    public partial Brush ProgressForegroundBrush { get; set; } = new SolidColorBrush(AppColors.PrimaryOrange);

    [ObservableProperty]
    public partial Brush TankBackgroundBrush { get; set; } = new SolidColorBrush(AppColors.AlmostWhiteGray);

    [ObservableProperty]
    public partial Brush TankWaterBrush { get; set; } = new SolidColorBrush(AppColors.WarningAmber);

    [ObservableProperty]
    public partial Brush TankBubbleBrush { get; set; } = new SolidColorBrush(Color.FromArgb(90, 255, 255, 255));

    public string FullName => Model.FullName;

    public string CodeText => $"{MemberIdLabelText} {Model.Code}";

    public string PhoneNumber => Model.PhoneNumber;

    public string MembershipRankName => Model.MembershipRank?.Name ?? string.Empty;

    public int ProgressPercentage => Model.ProgressPercentage;

    public IconState EditIconState { get; } = new()
    {
        Kind = IconKind.Update,
        Size = 18,
        AlwaysFilled = true,
    };

    public IconState DeleteIconState { get; } = new()
    {
        Kind = IconKind.Delete,
        Size = 18,
        AlwaysFilled = true,
    };

    public IAsyncRelayCommand EditCommand { get; }

    public IAsyncRelayCommand DeleteCommand { get; }

    protected override void RefreshLocalizedText()
    {
        TotalSpentLabelText = LocalizationService.GetString("MemberCardTotalSpentLabelText");
        MemberIdLabelText = LocalizationService.GetString("MemberCardIdLabelText");
        RefreshPresentation();
    }

    public new void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        Model.PropertyChanged -= HandleModelPropertyChanged;
        _isDisposed = true;
        base.Dispose();
    }

    private void HandleModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(MemberModel.Code):
                OnPropertyChanged(nameof(CodeText));
                break;
            case nameof(MemberModel.FullName):
                OnPropertyChanged(nameof(FullName));
                break;
            case nameof(MemberModel.PhoneNumber):
                OnPropertyChanged(nameof(PhoneNumber));
                break;
            case nameof(MemberModel.TotalSpentAmount):
            case nameof(MemberModel.MembershipRank):
            case nameof(MemberModel.ProgressPercentage):
            case nameof(MemberModel.NextMembershipRank):
                RefreshPresentation();
                break;
        }
    }

    private Task ExecuteEditAsync()
    {
        return _editAction?.Invoke(Model) ?? Task.CompletedTask;
    }

    private Task ExecuteDeleteAsync()
    {
        return _deleteAction?.Invoke(Model) ?? Task.CompletedTask;
    }

    private void RefreshPresentation()
    {
        Color baseColor = ParseColor(Model.MembershipRank?.Color, AppColors.WarningAmber);

        TotalSpentText = LocalizationService.FormatCurrency(Model.TotalSpentAmount);
        ProgressPercentageText = string.Format(LocalizationService.Culture, "{0}%", Model.ProgressPercentage);
        PackageBadgeBackground = new SolidColorBrush(baseColor);
        PackageBadgeForeground = new SolidColorBrush(AppColors.Black);
        ProgressForegroundBrush = new SolidColorBrush(baseColor);
        TankWaterBrush = new SolidColorBrush(baseColor);
        TankBubbleBrush = new SolidColorBrush(Color.FromArgb(70, 255, 255, 255));
        TankBackgroundBrush = new SolidColorBrush(Lighten(baseColor, 0.76));

        OnPropertyChanged(nameof(FullName));
        OnPropertyChanged(nameof(CodeText));
        OnPropertyChanged(nameof(PhoneNumber));
        OnPropertyChanged(nameof(MembershipRankName));
        OnPropertyChanged(nameof(ProgressPercentage));
    }

    private static Color ParseColor(string? hexColor, Color fallback)
    {
        if (string.IsNullOrWhiteSpace(hexColor))
        {
            return fallback;
        }

        string normalizedHex = hexColor.Trim().TrimStart('#');
        if (normalizedHex.Length == 6 && uint.TryParse(normalizedHex, System.Globalization.NumberStyles.HexNumber, null, out uint rgb))
        {
            byte red = (byte)((rgb & 0xFF0000) >> 16);
            byte green = (byte)((rgb & 0x00FF00) >> 8);
            byte blue = (byte)(rgb & 0x0000FF);
            return Color.FromArgb(0xFF, red, green, blue);
        }

        return fallback;
    }

    private static Color Lighten(Color color, double amount)
    {
        byte Blend(byte channel) => (byte)(channel + ((255 - channel) * amount));

        return Color.FromArgb(color.A, Blend(color.R), Blend(color.G), Blend(color.B));
    }
}
