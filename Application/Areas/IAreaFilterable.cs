using Domain.Enums;

namespace Application.Areas;

public interface IAreaFilterable
{
    PlayAreaType PlayAreaType { get; }

    PlayAreaStatus Status { get; }

    int MaxCapacity { get; }

    int Capacity { get; }

    decimal HourlyPrice { get; }

    DateTime? StartTime { get; }

    DateTime? CheckInDateTime { get; }
}
