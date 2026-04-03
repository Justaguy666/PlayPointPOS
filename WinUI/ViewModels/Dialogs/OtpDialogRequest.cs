using System;
using System.Threading.Tasks;

namespace WinUI.ViewModels.Dialogs;

public sealed class OtpDialogRequest
{
    public OtpDialogMode Mode { get; init; } = OtpDialogMode.ResetPassword;

    public string PendingEmail { get; init; } = string.Empty;

    public Func<Task>? OnVerifiedAsync { get; init; }
}
