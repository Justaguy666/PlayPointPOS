using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinUI.UIModels.Enums;
using WinUI.UIModels.Management;

namespace WinUI.ViewModels.Dialogs.Management;

public sealed class MemberDialogRequest
{
    public UpsertDialogMode Mode { get; init; } = UpsertDialogMode.Add;

    public MemberModel? Model { get; init; }

    public IReadOnlyList<MembershipRank>? AvailableMembershipRanks { get; init; }

    public Func<MemberModel, Task>? OnSubmittedAsync { get; init; }
}
