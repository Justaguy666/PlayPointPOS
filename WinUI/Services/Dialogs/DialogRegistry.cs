using System;
using System.Collections.Generic;
using System.Linq;
using Application.Services;
using Microsoft.UI.Xaml.Controls;

namespace WinUI.Services.Dialogs;

public sealed class DialogRegistry : IDialogFactory
{
    private readonly IReadOnlyDictionary<DialogKey, IDialogDefinition> _buildersByKey;
    private readonly IReadOnlyDictionary<Type, IDialogDefinition> _buildersByRequestType;

    public DialogRegistry(IEnumerable<IDialogDefinition> builders)
    {
        ArgumentNullException.ThrowIfNull(builders);

        IDialogDefinition[] registeredBuilders = builders.ToArray();
        _buildersByKey = registeredBuilders.ToDictionary(builder => builder.Key);
        _buildersByRequestType = registeredBuilders
            .Where(builder => builder.RequestType is not null)
            .ToDictionary(builder => builder.RequestType!);
    }

    public ContentDialog? Create(DialogKey dialogKey, object? parameter)
    {
        return _buildersByKey.TryGetValue(dialogKey, out IDialogDefinition? builder)
            ? builder.Create(parameter)
            : null;
    }

    public ContentDialog Create<TRequest>(TRequest request)
        where TRequest : notnull
    {
        ArgumentNullException.ThrowIfNull(request);

        Type requestType = typeof(TRequest);
        if (!_buildersByRequestType.TryGetValue(requestType, out IDialogDefinition? builder))
        {
            throw new InvalidOperationException(
                $"No dialog builder is registered for request type {requestType.Name}.");
        }

        return builder.Create(request);
    }
}
