namespace Application.Areas;

public sealed record AreaReservationRequest
{
    public string CustomerName { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;

    public string? MemberId { get; init; }

    public DateTime CheckInDateTime { get; init; }

    public int Capacity { get; init; }
}
