using Application.Services;

namespace Infrastructure.Services;

/// <summary>
/// Implementation of IDateTimeService.
/// Provides current date/time to application, allowing for mocking in tests.
/// </summary>
public class DateTimeService : IDateTimeService
{
    /// <summary>
    /// Gets the current local date and time.
    /// </summary>
    public DateTime GetNow() => DateTime.Now;
}
