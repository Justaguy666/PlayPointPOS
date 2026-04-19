using Application.Services;
using System;
using System.Threading.Tasks;
using WinUI.UIModels.Management;
using WinUI.ViewModels.UserControls.Members;

namespace WinUI.Services.Factories;

public sealed class MemberCardControlViewModelFactory
{
    private readonly ILocalizationService _localizationService;

    public MemberCardControlViewModelFactory(ILocalizationService localizationService)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    }

    public MemberCardControlViewModel Create(
        MemberModel model,
        Func<MemberModel, Task>? editAction,
        Func<MemberModel, Task>? deleteAction)
    {
        return new MemberCardControlViewModel(
            _localizationService,
            model,
            editAction,
            deleteAction);
    }
}
