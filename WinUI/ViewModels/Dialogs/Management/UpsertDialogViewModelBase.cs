using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WinUI.UIModels.Enums;
using Application.Services;
using WinUI.UIModels;

namespace WinUI.ViewModels.Dialogs.Management;

public abstract partial class UpsertDialogViewModelBase : LocalizedViewModelBase
{
    public UpsertDialogMode Mode { get; }

    public bool IsEdit => Mode == UpsertDialogMode.Edit;

    [ObservableProperty]
    private string _submitButtonText;

    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private IconState _icon;

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
