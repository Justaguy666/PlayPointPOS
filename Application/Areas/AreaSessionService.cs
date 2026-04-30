using Application.Services.Areas;
using Domain.Enums;

namespace Application.Areas;

public sealed class AreaSessionService : IAreaSessionService
{
    public int ClampCapacity(int capacity, int maxCapacity)
    {
        return Math.Clamp(capacity, 1, GetCapacityUpperBound(maxCapacity));
    }

    public bool IsCapacityValid(int capacity, int maxCapacity)
    {
        return capacity > 0 && capacity <= GetCapacityUpperBound(maxCapacity);
    }

    public bool CanCheckInReservation(IAreaSessionState area, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(area);

        return area.CheckInDateTime is DateTime checkInDateTime
            && now >= checkInDateTime;
    }

    public void Reserve(IAreaSessionState area, AreaReservationRequest request)
    {
        ArgumentNullException.ThrowIfNull(area);
        ArgumentNullException.ThrowIfNull(request);

        area.CustomerName = request.CustomerName.Trim();
        area.PhoneNumber = request.PhoneNumber.Trim();
        area.MemberId = request.MemberId;
        area.CheckInDateTime = request.CheckInDateTime;
        area.Capacity = ClampCapacity(request.Capacity, GetAreaMaxCapacity(area));
        area.StartTime = null;
        area.IsSessionPaused = false;
        area.SessionPausedAt = null;
        area.SessionPausedDuration = TimeSpan.Zero;
        area.TotalAmount = 0m;
        area.Status = PlayAreaStatus.Reserved;
    }

    public void StartSession(IAreaSessionState area, StartAreaSessionRequest request, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(area);
        ArgumentNullException.ThrowIfNull(request);

        area.CustomerName = request.CustomerName.Trim();
        area.PhoneNumber = request.PhoneNumber.Trim();
        area.MemberId = request.MemberId;
        area.CheckInDateTime = null;
        area.Capacity = ClampCapacity(request.Capacity, GetAreaMaxCapacity(area));
        area.StartTime = NormalizeToUtc(utcNow);
        area.IsSessionPaused = false;
        area.SessionPausedAt = null;
        area.SessionPausedDuration = TimeSpan.Zero;
        area.TotalAmount = 0m;
        area.Status = PlayAreaStatus.Rented;
    }

    public void CheckInReservation(IAreaSessionState area, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(area);

        area.StartTime = NormalizeToUtc(utcNow);
        area.IsSessionPaused = false;
        area.SessionPausedAt = null;
        area.SessionPausedDuration = TimeSpan.Zero;
        area.TotalAmount = 0m;
        area.CheckInDateTime = null;
        area.Status = PlayAreaStatus.Rented;
    }

    public void CancelReservation(IAreaSessionState area)
    {
        ArgumentNullException.ThrowIfNull(area);

        ClearCustomerSession(area);
        area.Status = PlayAreaStatus.Available;
    }

    public void ToggleSession(IAreaSessionState area, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(area);

        if (area.IsSessionPaused)
        {
            ResumeSession(area, utcNow);
            return;
        }

        PauseSession(area, utcNow);
    }

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

    public AreaPaymentSummary CalculatePaymentSummary(IAreaSessionState area, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(area);

        TimeSpan elapsedTime = GetSessionElapsedTime(area, utcNow);
        decimal areaFee = CalculateAreaSessionTotal(area, elapsedTime);
        decimal productFee = 0m;
        decimal gameFee = 0m;
        decimal deposit = 0m;
        decimal discount = 0m;
        decimal total = areaFee + productFee + gameFee - deposit - discount;

        return new AreaPaymentSummary
        {
            ElapsedTime = elapsedTime,
            AreaFee = areaFee,
            ProductFee = productFee,
            GameFee = gameFee,
            Deposit = deposit,
            Discount = discount,
            Total = total,
        };
    }

    public void CompletePayment(IAreaSessionState area)
    {
        ArgumentNullException.ThrowIfNull(area);

        ClearCustomerSession(area);
        area.Status = PlayAreaStatus.Available;
    }

    private static void ClearCustomerSession(IAreaSessionState area)
    {
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
    }

    private static int GetCapacityUpperBound(int maxCapacity)
    {
        return maxCapacity > 0 ? Math.Max(1, maxCapacity) : int.MaxValue;
    }

    private static int GetAreaMaxCapacity(IAreaSessionState area)
    {
        return area is IAreaCapacity areaCapacity ? areaCapacity.MaxCapacity : 0;
    }

    private static DateTime NormalizeToUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc
            ? value
            : value.ToUniversalTime();
    }
}
