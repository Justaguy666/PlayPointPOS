namespace Application.Services;

public sealed record AuthOperationResult
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;
}
