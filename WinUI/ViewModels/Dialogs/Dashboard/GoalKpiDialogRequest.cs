using System;
using System.Threading.Tasks;

namespace WinUI.ViewModels.Dialogs.Dashboard;

public sealed class GoalKpiDialogRequest
{
    public int RevenueGoalValue { get; init; }

    public int CustomerGoalValue { get; init; }

    public int MemberGoalValue { get; init; }

    public Func<int, int, int, Task>? OnSubmittedAsync { get; init; }
}
