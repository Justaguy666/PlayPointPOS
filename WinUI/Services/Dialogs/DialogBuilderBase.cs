using System;
using Microsoft.UI.Xaml.Controls;

namespace WinUI.Services.Dialogs;

public abstract class ParameterlessDialogBuilder : IDialogDefinition
{
    public abstract DialogKey Key { get; }

    public Type? RequestType => null;

    public ContentDialog Create(object? request)
    {
        if (request is not null)
        {
            throw CreateRequestTypeException(request.GetType());
        }

        return CreateCore();
    }

    protected abstract ContentDialog CreateCore();

    protected InvalidOperationException CreateRequestTypeException(Type actualType)
    {
        return new InvalidOperationException(
            $"{Key} dialog does not accept request type {actualType.Name}.");
    }
}

public abstract class OptionalDialogBuilder<TRequest> : IDialogDefinition
    where TRequest : class
{
    public abstract DialogKey Key { get; }

    public Type? RequestType => typeof(TRequest);

    public ContentDialog Create(object? request)
    {
        if (request is null)
        {
            return CreateCore(null);
        }

        if (request is TRequest typedRequest)
        {
            return CreateCore(typedRequest);
        }

        throw CreateRequestTypeException(typeof(TRequest), request.GetType());
    }

    protected abstract ContentDialog CreateCore(TRequest? request);

    protected InvalidOperationException CreateRequestTypeException(Type expectedType, Type actualType)
    {
        return new InvalidOperationException(
            $"{Key} dialog expects request type {expectedType.Name}, but received {actualType.Name}.");
    }
}

public abstract class RequiredDialogBuilder<TRequest> : IDialogBuilder<TRequest>
    where TRequest : class
{
    public abstract DialogKey Key { get; }

    public Type? RequestType => typeof(TRequest);

    public ContentDialog Create(object? request)
    {
        if (request is TRequest typedRequest)
        {
            return Create(typedRequest);
        }

        throw CreateRequestTypeException(typeof(TRequest), request?.GetType());
    }

    public abstract ContentDialog Create(TRequest request);

    protected InvalidOperationException CreateRequestTypeException(Type expectedType, Type? actualType)
    {
        string actualName = actualType?.Name ?? "null";
        return new InvalidOperationException(
            $"{Key} dialog requires request type {expectedType.Name}, but received {actualName}.");
    }
}
