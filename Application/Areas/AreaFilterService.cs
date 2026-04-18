using Application.Services.Areas;

namespace Application.Areas;

public sealed class AreaFilterService : IAreaFilterService
{
    public IReadOnlyList<TArea> Apply<TArea>(IEnumerable<TArea> areas, PlayAreaFilter filter, string timeZone)
        where TArea : IAreaFilterable
    {
        ArgumentNullException.ThrowIfNull(areas);
        ArgumentNullException.ThrowIfNull(filter);

        return areas
            .Where(area =>
                MatchesAreaType(area, filter)
                && MatchesStatus(area, filter)
                && MatchesStartTime(area, filter, timeZone)
                && MatchesCapacity(area, filter)
                && MatchesHourlyPrice(area, filter))
            .ToList();
    }

    private static bool MatchesAreaType(IAreaFilterable area, PlayAreaFilter filter)
    {
        return filter.AreaType is null || area.PlayAreaType == filter.AreaType.Value;
    }

    private static bool MatchesStatus(IAreaFilterable area, PlayAreaFilter filter)
    {
        return filter.Status is null || area.Status == filter.Status.Value;
    }

    private static bool MatchesStartTime(IAreaFilterable area, PlayAreaFilter filter, string timeZone)
    {
        if (filter.StartTimeFrom is null && filter.StartTimeTo is null)
        {
            return true;
        }

        TimeSpan? comparableStartTime = GetComparableStartTime(area, timeZone);
        if (!comparableStartTime.HasValue)
        {
            return false;
        }

        return (filter.StartTimeFrom is null || comparableStartTime.Value >= filter.StartTimeFrom.Value)
            && (filter.StartTimeTo is null || comparableStartTime.Value <= filter.StartTimeTo.Value);
    }

    private static bool MatchesCapacity(IAreaFilterable area, PlayAreaFilter filter)
    {
        int comparableCapacity = area.MaxCapacity > 0 ? area.MaxCapacity : area.Capacity;

        return (filter.CapacityMin is null || comparableCapacity >= filter.CapacityMin.Value)
            && (filter.CapacityMax is null || comparableCapacity <= filter.CapacityMax.Value);
    }

    private static bool MatchesHourlyPrice(IAreaFilterable area, PlayAreaFilter filter)
    {
        return (filter.HourlyPriceMin is null || area.HourlyPrice >= filter.HourlyPriceMin.Value)
            && (filter.HourlyPriceMax is null || area.HourlyPrice <= filter.HourlyPriceMax.Value);
    }

    private static TimeSpan? GetComparableStartTime(IAreaFilterable area, string timeZone)
    {
        if (area.StartTime is DateTime startTime)
        {
            return ConvertUtcToConfiguredTime(startTime, timeZone).TimeOfDay;
        }

        if (area.CheckInDateTime is DateTime checkInDateTime)
        {
            return checkInDateTime.TimeOfDay;
        }

        return null;
    }

    private static DateTime ConvertUtcToConfiguredTime(DateTime value, string timeZone)
    {
        DateTime utcValue = value.Kind == DateTimeKind.Utc
            ? value
            : value.ToUniversalTime();
        return utcValue.AddHours(ParseTimeZoneOffset(timeZone));
    }

    private static int ParseTimeZoneOffset(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        string normalized = value.Trim().ToUpperInvariant().Replace("UTC", string.Empty);
        return int.TryParse(normalized, out int offset) ? offset : 0;
    }
}
