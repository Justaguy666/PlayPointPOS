using System;
using Application.Services;
using Microsoft.UI.Xaml.Controls;

namespace WinUI.Services.Dialogs;

public interface IDialogDefinition
{
    DialogKey Key { get; }

    Type? RequestType { get; }

    ContentDialog Create(object? request);
}

public interface IDialogBuilder<in TRequest> : IDialogDefinition
    where TRequest : notnull
{
    ContentDialog Create(TRequest request);
}
