namespace Application.Areas;

public sealed record StartAreaSessionRequest
{
    public string CustomerName { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;

    public string? MemberId { get; init; }

    public int Capacity { get; init; }
}
