using System;
using System.Threading.Tasks;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using WinUI.UIModels;
using WinUI.UIModels.Enums;

namespace WinUI.ViewModels.Dialogs.Management;

public class MembershipPackageEditDialogViewModel : LocalizedViewModelBase
{
    private MembershipPackageItemViewModel? _item;
    private Func<MembershipPackageItemViewModel, string, string, string, Color, Task>? _onSubmittedAsync;

    private string _titleText = string.Empty;
    private string _namePlaceholderText = string.Empty;
    private string _minSpentPlaceholderText = string.Empty;
    private string _discountPlaceholderText = string.Empty;
    private string _colorButtonText = string.Empty;
    private string _saveButtonText = "Lưu";
    private string _cancelButtonText = "Hủy";
    private string _closeTooltipText = string.Empty;
    private string _editName = string.Empty;
    private string _editMinSpentText = string.Empty;
    private string _editDiscountText = string.Empty;
    private string _errorMessage = string.Empty;
    private Color _editColor = MembershipPackageDialogViewModel.ParseColor("#F09A44");
    private Brush _editColorBrush = new SolidColorBrush(MembershipPackageDialogViewModel.ParseColor("#F09A44"));

    public MembershipPackageEditDialogViewModel(ILocalizationService localizationService)
        : base(localizationService)
    {
        SaveCommand = new AsyncRelayCommand(SaveAsync, () => CanSave);
        CancelCommand = new RelayCommand(Close);
        CloseCommand = new RelayCommand(Close);
        RefreshLocalizedText();
    }

    public IconState PaintIconState { get; } = new()
    {
        Kind = IconKind.Paint,
        Size = 16,
        AlwaysFilled = true,
    };

    public IAsyncRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }
    public IRelayCommand CloseCommand { get; }

    public string TitleText
    {
        get => _titleText;
        set => SetProperty(ref _titleText, value);
    }

    public string NamePlaceholderText
    {
        get => _namePlaceholderText;
        set => SetProperty(ref _namePlaceholderText, value);
    }

    public string MinSpentPlaceholderText
    {
        get => _minSpentPlaceholderText;
        set => SetProperty(ref _minSpentPlaceholderText, value);
    }

    public string DiscountPlaceholderText
    {
        get => _discountPlaceholderText;
        set => SetProperty(ref _discountPlaceholderText, value);
    }

    public string ColorButtonText
    {
        get => _colorButtonText;
        set => SetProperty(ref _colorButtonText, value);
    }

    public string SaveButtonText
    {
        get => _saveButtonText;
        set => SetProperty(ref _saveButtonText, value);
    }

    public string CancelButtonText
    {
        get => _cancelButtonText;
        set => SetProperty(ref _cancelButtonText, value);
    }

    public string CloseTooltipText
    {
        get => _closeTooltipText;
        set => SetProperty(ref _closeTooltipText, value);
    }

    public string EditName
    {
        get => _editName;
        set
        {
            if (SetProperty(ref _editName, value))
            {
                OnPropertyChanged(nameof(CanSave));
                SaveCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string EditMinSpentText
    {
        get => _editMinSpentText;
        set
        {
            if (SetProperty(ref _editMinSpentText, value))
            {
                OnPropertyChanged(nameof(CanSave));
                SaveCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string EditDiscountText
    {
        get => _editDiscountText;
        set
        {
            if (SetProperty(ref _editDiscountText, value))
            {
                OnPropertyChanged(nameof(CanSave));
                SaveCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

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

    public Brush EditColorBrush
    {
        get => _editColorBrush;
        set => SetProperty(ref _editColorBrush, value);
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool CanSave =>
        !string.IsNullOrWhiteSpace(EditName)
        && TryParseOptionalDecimal(EditMinSpentText, out _)
        && TryParseOptionalDecimal(EditDiscountText, out _);

    public event Action? CloseRequested;

    public void Configure(MembershipPackageEditDialogRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        _item = request.Item;
        _onSubmittedAsync = request.OnSubmittedAsync;

        EditName = request.Item.Name;
        EditMinSpentText = request.Item.MembershipRank.MinSpentAmount.ToString("0.##", LocalizationService.Culture);
        EditDiscountText = (request.Item.MembershipRank.DiscountRate * 100m).ToString("0.##", LocalizationService.Culture);
        EditColor = MembershipPackageDialogViewModel.ParseColor(request.Item.MembershipRank.Color);
        ErrorMessage = string.Empty;
    }

    protected override void RefreshLocalizedText()
    {
        TitleText = LocalizationService.GetString("MembershipPackageDialogTitleText");
        NamePlaceholderText = LocalizationService.GetString("MembershipPackageDialogNamePlaceholderText");
        MinSpentPlaceholderText = LocalizationService.GetString("MembershipPackageDialogMinSpentPlaceholderText");
        DiscountPlaceholderText = LocalizationService.GetString("MembershipPackageDialogDiscountPlaceholderText");
        ColorButtonText = LocalizationService.GetString("MembershipPackageDialogColorButtonText");
        CloseTooltipText = LocalizationService.GetString("CloseTooltipText");

        string applyText = LocalizationService.GetString("MemberFilterDialogApplyButtonText");
        SaveButtonText = string.IsNullOrWhiteSpace(applyText) ? "Lưu" : applyText;

        string resetText = LocalizationService.GetString("MemberFilterDialogResetButtonText");
        CancelButtonText = string.IsNullOrWhiteSpace(resetText) ? "Hủy" : resetText;

        SaveCommand.NotifyCanExecuteChanged();
    }

    private async Task SaveAsync()
    {
        if (_item is null)
        {
            return;
        }

        if (!CanSave)
        {
            ErrorMessage = LocalizationService.GetString("MembershipPackageDialogInvalidInputText");
            return;
        }

        if (_onSubmittedAsync is not null)
        {
            await _onSubmittedAsync(_item, EditName, EditMinSpentText, EditDiscountText, EditColor);
        }

        CloseRequested?.Invoke();
    }

    private void Close()
    {
        CloseRequested?.Invoke();
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
}
