using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using WinUI.UIModels;
using WinUI.UIModels.Enums;

namespace WinUI.ViewModels.Dialogs.Management;

public abstract partial class UpsertDialogViewModelBase : LocalizedViewModelBase
{
    public UpsertDialogMode Mode { get; }

    public bool IsEdit => Mode == UpsertDialogMode.Edit;

    [ObservableProperty]
    public partial string SubmitButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial IconState Icon { get; set; } = new();

    protected UpsertDialogViewModelBase(
        ILocalizationService localizationService,
        UpsertDialogMode mode)
        : base(localizationService)
    {
        Mode = mode;
        Icon = new IconState { Kind = IsEdit ? IconKind.Update : IconKind.Add };

        RefreshLocalizedText();
    }

    protected abstract string CreateTitleLocKey { get; }
    protected abstract string EditTitleLocKey { get; }

    protected override void RefreshLocalizedText()
    {
        SubmitButtonText = IsEdit
            ? LocalizationService.GetString("SaveButtonText")
            : LocalizationService.GetString("CreateButtonText");

        Title = IsEdit
            ? LocalizationService.GetString(EditTitleLocKey)
            : LocalizationService.GetString(CreateTitleLocKey);
    }

    public abstract Task SaveAsync();
}
