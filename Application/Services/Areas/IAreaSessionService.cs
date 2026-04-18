using Application.Areas;

namespace Application.Services.Areas;

public interface IAreaSessionService
{
    TimeSpan GetSessionElapsedTime(IAreaSessionState area, DateTime utcNow);

    void PauseSession(IAreaSessionState area, DateTime utcNow);

    void ResumeSession(IAreaSessionState area, DateTime utcNow);

    decimal CalculateAreaSessionTotal(IAreaSessionState area, TimeSpan elapsedTime);

    void CompletePayment(IAreaSessionState area);
}
