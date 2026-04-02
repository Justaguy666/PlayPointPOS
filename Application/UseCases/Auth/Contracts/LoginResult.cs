using Domain.Entities;

namespace Application.UseCases.Auth.Contracts;

/// <summary>
/// Result of a login attempt.
/// Contains success status, the authenticated account, and a user-friendly message.
/// </summary>
public class LoginResult
{
    /// <summary>
    /// Whether the login was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The authenticated account (null if login failed).
    /// </summary>
    public Account? Account { get; set; }

    /// <summary>
    /// User-friendly message (e.g., "Login successful" or "Invalid credentials").
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
