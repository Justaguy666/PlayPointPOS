using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media;
using WinUI.Resources;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels;

namespace WinUI.ViewModels.UserControls;

public sealed partial class NotificationControlViewModel : LocalizedViewModelBase
{
    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CloseTooltipText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial IconState IconState { get; set; } = new IconState { Kind = IconKind.Info, Size = 24, AlwaysFilled = true };

    [ObservableProperty]
    public partial Brush IconBackground { get; set; } = AppResourceLookup.GetBrush("InfoBlueLightBrush", AppColors.InfoBlueLight);

    [ObservableProperty]
    public partial bool IsVisible { get; set; }

    [ObservableProperty]
    public partial int DisplayVersion { get; set; }

    public IRelayCommand CloseCommand { get; }

    public NotificationControlViewModel(ILocalizationService localizationService)
        : base(localizationService)
    {
        CloseCommand = new RelayCommand(Close);
        RefreshLocalizedText();
        ApplyNotificationType(NotificationType.Info);
    }

    public void Show(string title, string message, NotificationType type)
    {
        Title = title;
        Message = message;
        ApplyNotificationType(type);
        IsVisible = true;
        DisplayVersion++;
    }

    public void Close()
    {
        IsVisible = false;
    }

    protected override void RefreshLocalizedText()
    {
        CloseTooltipText = LocalizationService.GetString("CloseTooltipText");

        if (DisplayVersion == 0)
        {
            Title = LocalizationService.GetString("NotificationDefaultTitle");
            Message = LocalizationService.GetString("NotificationDefaultMessage");
        }
    }

    private void ApplyNotificationType(NotificationType type)
    {
        switch (type)
        {
            case NotificationType.Success:
                IconBackground = AppResourceLookup.GetBrush("SuccessGreenLightBrush", AppColors.SuccessGreenLight);
                IconState = new IconState { Kind = IconKind.Success, Size = 24, AlwaysFilled = true };
                break;
            case NotificationType.Error:
                IconBackground = AppResourceLookup.GetBrush("ErrorRedLightBrush", AppColors.ErrorRedLight);
                IconState = new IconState { Kind = IconKind.Error, Size = 24, AlwaysFilled = true };
                break;
            case NotificationType.Warning:
                IconBackground = AppResourceLookup.GetBrush("WarningAmberLightBrush", AppColors.WarningAmberLight);
                IconState = new IconState { Kind = IconKind.Warning, Size = 24, AlwaysFilled = true };
                break;
            default:
                IconBackground = AppResourceLookup.GetBrush("InfoBlueLightBrush", AppColors.InfoBlueLight);
                IconState = new IconState { Kind = IconKind.Info, Size = 24, AlwaysFilled = true };
                break;
        }
    }
}
