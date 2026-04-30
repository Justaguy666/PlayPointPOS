using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Application.Members;
using Application.Services;
using WinUI.UIModels.Enums;
using WinUI.UIModels.Management;
using WinUI.ViewModels.Dialogs.Management;

namespace WinUI.Services.Management;

public sealed class MemberManagementDialogCoordinator
{
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;

    public MemberManagementDialogCoordinator(
        IDialogService dialogService,
        ILocalizationService localizationService,
        INotificationService notificationService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    public Task OpenFilterAsync(
        IReadOnlyList<MembershipRank> availableMembershipRanks,
        MemberFilter initialCriteria,
        Func<MemberFilter, Task> onSubmittedAsync)
    {
        return _dialogService.ShowDialogAsync(
            new MemberFilterDialogRequest
            {
                AvailableMembershipRanks = availableMembershipRanks,
                InitialCriteria = initialCriteria,
                OnSubmittedAsync = onSubmittedAsync,
            });
    }

    public Task OpenAddAsync(
        MemberModel draft,
        IReadOnlyList<MembershipRank> availableMembershipRanks,
        Func<MemberModel, Task> onSubmittedAsync)
    {
        return OpenUpsertAsync(UpsertDialogMode.Add, draft, availableMembershipRanks, onSubmittedAsync);
    }

    public Task OpenEditAsync(
        MemberModel member,
        IReadOnlyList<MembershipRank> availableMembershipRanks,
        Func<MemberModel, Task> onSubmittedAsync)
    {
        return OpenUpsertAsync(UpsertDialogMode.Edit, member, availableMembershipRanks, onSubmittedAsync);
    }

    public Task OpenMembershipPackagesAsync(
        ObservableCollection<MembershipRank> membershipRanks,
        Func<MembershipRank, Task> onMembershipRankAddedAsync,
        Func<MembershipRank, Task> onMembershipRankDeletedAsync,
        Func<MembershipRank, Task> onMembershipRankUpdatedAsync)
    {
        return _dialogService.ShowDialogAsync(
            new MembershipPackageDialogRequest
            {
                MembershipRanks = membershipRanks,
                OnMembershipRankAddedAsync = onMembershipRankAddedAsync,
                OnMembershipRankDeletedAsync = onMembershipRankDeletedAsync,
                OnMembershipRankUpdatedAsync = onMembershipRankUpdatedAsync,
            });
    }

    public Task<bool> ConfirmDeleteAsync()
    {
        return _dialogService.ShowConfirmationAsync(
            titleKey: "ConfirmDeleteMemberTitle",
            messageKey: "ConfirmDeleteMemberMessage",
            confirmButtonTextKey: "ConfirmDeleteMemberButton",
            cancelButtonTextKey: "CancelButtonText");
    }

    public Task NotifyCreatedAsync(MemberModel member)
    {
        return NotifyAsync("MemberCreatedSuccessTitle", "MemberCreatedSuccessMessage", member.FullName);
    }

    public Task NotifyUpdatedAsync(MemberModel member)
    {
        return NotifyAsync("MemberUpdatedSuccessTitle", "MemberUpdatedSuccessMessage", member.FullName);
    }

    public Task NotifyDeletedAsync(MemberModel member)
    {
        return NotifyAsync("MemberDeletedSuccessTitle", "MemberDeletedSuccessMessage", member.FullName);
    }

    private Task OpenUpsertAsync(
        UpsertDialogMode mode,
        MemberModel member,
        IReadOnlyList<MembershipRank> availableMembershipRanks,
        Func<MemberModel, Task> onSubmittedAsync)
    {
        return _dialogService.ShowDialogAsync(
            new MemberDialogRequest
            {
                Mode = mode,
                Model = member,
                AvailableMembershipRanks = availableMembershipRanks,
                OnSubmittedAsync = onSubmittedAsync,
            });
    }

    private Task NotifyAsync(string titleKey, string messageKey, string name)
    {
        return _notificationService.SendAsync(
            _localizationService.GetString(titleKey),
            string.Format(
                _localizationService.Culture,
                _localizationService.GetString(messageKey),
                name),
            NotificationType.Success);
    }
}
