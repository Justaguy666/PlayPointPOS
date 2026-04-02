using Domain.Entities;

namespace Application.UseCases.Auth.Contracts;

/// <summary>
/// Result of a registration attempt.
/// Contains success status, the created account, and a user-friendly message.
/// </summary>
public class RegisterResult
{
    /// <summary>
    /// Whether the registration was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The newly created account (null if registration failed).
    /// </summary>
    public Account? Account { get; set; }

    /// <summary>
    /// User-friendly message (e.g., "Registration successful" or "Email already registered").
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
