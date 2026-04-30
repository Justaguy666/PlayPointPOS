using Application.Areas;

namespace Application.Services.Areas;

public interface IAreaSessionService
{
    int ClampCapacity(int capacity, int maxCapacity);

    bool IsCapacityValid(int capacity, int maxCapacity);

    bool CanCheckInReservation(IAreaSessionState area, DateTime now);

    void Reserve(IAreaSessionState area, AreaReservationRequest request);

    void StartSession(IAreaSessionState area, StartAreaSessionRequest request, DateTime utcNow);

    void CheckInReservation(IAreaSessionState area, DateTime utcNow);

    void CancelReservation(IAreaSessionState area);

    void ToggleSession(IAreaSessionState area, DateTime utcNow);

    TimeSpan GetSessionElapsedTime(IAreaSessionState area, DateTime utcNow);

    void PauseSession(IAreaSessionState area, DateTime utcNow);

    void ResumeSession(IAreaSessionState area, DateTime utcNow);

    decimal CalculateAreaSessionTotal(IAreaSessionState area, TimeSpan elapsedTime);

    AreaPaymentSummary CalculatePaymentSummary(IAreaSessionState area, DateTime utcNow);

    void CompletePayment(IAreaSessionState area);
}
