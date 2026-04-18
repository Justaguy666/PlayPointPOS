using Application.Services.Areas;
using Domain.Enums;

namespace Application.Areas;

public sealed class AreaSessionService : IAreaSessionService
{
    public TimeSpan GetSessionElapsedTime(IAreaSessionState area, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(area);

        if (area.StartTime is not DateTime startTime)
        {
            return TimeSpan.Zero;
        }

        DateTime effectiveNowUtc = area.IsSessionPaused && area.SessionPausedAt is DateTime pausedAt
            ? NormalizeToUtc(pausedAt)
            : NormalizeToUtc(utcNow);

        TimeSpan elapsedTime = effectiveNowUtc - NormalizeToUtc(startTime) - area.SessionPausedDuration;
        return elapsedTime < TimeSpan.Zero ? TimeSpan.Zero : elapsedTime;
    }

    public void PauseSession(IAreaSessionState area, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(area);

        if (area.IsSessionPaused || area.StartTime is null)
        {
            return;
        }

        area.SessionPausedAt = NormalizeToUtc(utcNow);
        area.IsSessionPaused = true;
    }

    public void ResumeSession(IAreaSessionState area, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(area);

        if (!area.IsSessionPaused)
        {
            return;
        }

        DateTime resumedAtUtc = NormalizeToUtc(utcNow);
        if (area.SessionPausedAt is DateTime pausedAt)
        {
            TimeSpan pausedDuration = resumedAtUtc - NormalizeToUtc(pausedAt);
            if (pausedDuration > TimeSpan.Zero)
            {
                area.SessionPausedDuration += pausedDuration;
            }
        }

        area.SessionPausedAt = null;
        area.IsSessionPaused = false;
    }

    public decimal CalculateAreaSessionTotal(IAreaSessionState area, TimeSpan elapsedTime)
    {
        ArgumentNullException.ThrowIfNull(area);

        if (area.HourlyPrice <= 0m || elapsedTime <= TimeSpan.Zero)
        {
            return 0m;
        }

        int billableHalfHours = Math.Max(1, (int)Math.Floor(elapsedTime.TotalMinutes / 30d));
        return area.HourlyPrice * billableHalfHours / 2m;
    }

    public void CompletePayment(IAreaSessionState area)
    {
        ArgumentNullException.ThrowIfNull(area);

        area.CustomerName = string.Empty;
        area.PhoneNumber = string.Empty;
        area.MemberId = null;
        area.CheckInDateTime = null;
        area.Capacity = 0;
        area.StartTime = null;
        area.IsSessionPaused = false;
        area.SessionPausedAt = null;
        area.SessionPausedDuration = TimeSpan.Zero;
        area.TotalAmount = 0m;
        area.Status = PlayAreaStatus.Available;
    }

    private static DateTime NormalizeToUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc
            ? value
            : value.ToUniversalTime();
    }
}
