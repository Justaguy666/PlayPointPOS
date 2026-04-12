using System;

namespace WinUI.Helpers;

internal static class ReservationDateTimeHelper
{
    public const int MinuteIncrement = 5;

    public static DateTimeOffset GetMinimumSelectableDateTime()
    {
        var now = DateTimeOffset.Now;
        var roundedNow = new DateTimeOffset(
            now.Year,
            now.Month,
            now.Day,
            now.Hour,
            now.Minute,
            0,
            now.Offset);

        int remainder = roundedNow.Minute % MinuteIncrement;
        if (remainder != 0)
        {
            roundedNow = roundedNow.AddMinutes(MinuteIncrement - remainder);
        }
        else if (now.Second > 0 || now.Millisecond > 0)
        {
            roundedNow = roundedNow.AddMinutes(MinuteIncrement);
        }

        return roundedNow;
    }

    public static DateTimeOffset CoerceDate(DateTimeOffset selectedDate)
    {
        var minimumDate = GetMinimumSelectableDateTime();
        return selectedDate.Date < minimumDate.Date
            ? CreateDate(minimumDate)
            : CreateDate(selectedDate);
    }

    public static TimeSpan GetMinimumSelectableTime(DateTimeOffset selectedDate)
    {
        var minimumDate = GetMinimumSelectableDateTime();
        return selectedDate.Date == minimumDate.Date
            ? minimumDate.TimeOfDay
            : TimeSpan.Zero;
    }

    public static TimeSpan CoerceTime(DateTimeOffset selectedDate, TimeSpan selectedTime)
    {
        TimeSpan minimumTime = GetMinimumSelectableTime(selectedDate);
        return selectedTime < minimumTime ? minimumTime : selectedTime;
    }

    public static bool IsValidSelection(DateTimeOffset? selectedDate, TimeSpan? selectedTime)
    {
        if (!selectedDate.HasValue || !selectedTime.HasValue)
        {
            return false;
        }

        DateTimeOffset coercedDate = CoerceDate(selectedDate.Value);
        if (coercedDate.Date != selectedDate.Value.Date)
        {
            return false;
        }

        return selectedTime.Value >= GetMinimumSelectableTime(selectedDate.Value);
    }

    public static DateTimeOffset CreateDate(DateTimeOffset value)
        => new(value.Year, value.Month, value.Day, 0, 0, 0, value.Offset);
}
