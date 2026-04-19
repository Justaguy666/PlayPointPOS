using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Domain.Entities;

namespace WinUI.ViewModels.Dialogs.Management;

public sealed class MembershipPackageDialogRequest
{
    public required ObservableCollection<MembershipRank> MembershipRanks { get; init; }

    public Func<MembershipRank, System.Threading.Tasks.Task>? OnMembershipRankAddedAsync { get; init; }

    public Func<MembershipRank, Task>? OnMembershipRankDeletedAsync { get; init; }

    public Func<MembershipRank, Task>? OnMembershipRankUpdatedAsync { get; init; }
}
