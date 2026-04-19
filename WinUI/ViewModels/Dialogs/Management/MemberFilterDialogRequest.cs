using Application.Members;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WinUI.ViewModels.Dialogs.Management;

public sealed class MemberFilterDialogRequest
{
    public IReadOnlyList<MembershipRank>? AvailableMembershipRanks { get; init; }

    public MemberFilter? InitialCriteria { get; init; }

    public Func<MemberFilter, Task>? OnSubmittedAsync { get; init; }
}
