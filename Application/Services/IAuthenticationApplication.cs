using Application.UseCases.Auth.Contracts;

namespace Application.Services;

/// <summary>
/// Application-level facade for authentication operations.
/// Coordinates between Views, Use Cases, and Infrastructure.
/// This is the primary entry point for authentication business logic.
/// </summary>
public interface IAuthenticationApplication
{
    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="email">User's email</param>
    /// <param name="password">User's plain-text password (will be hashed for comparison)</param>
    /// <returns>LoginResult with success status and authenticated account (if successful)</returns>
    Task<LoginResult> LoginAsync(string email, string password);

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="email">User's email (must be unique)</param>
    /// <param name="password">User's password (will be hashed for storage)</param>
    /// <param name="language">User's preferred language (e.g., "en-US", "vi-VN")</param>
    /// <returns>RegisterResult with success status and message</returns>
    Task<RegisterResult> RegisterAsync(string email, string password, string language);
}
