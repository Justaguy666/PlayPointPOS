namespace Application.Services;

/// <summary>
/// Interface for password hashing and verification.
/// Implementations should use secure algorithms like BCrypt.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plain-text password securely.
    /// </summary>
    /// <param name="plainPassword">The plain-text password to hash</param>
    /// <returns>The hashed password (can be safely stored in database)</returns>
    string HashPassword(string plainPassword);

    /// <summary>
    /// Verifies a plain-text password against a stored hash.
    /// </summary>
    /// <param name="plainPassword">The plain-text password to verify</param>
    /// <param name="hash">The stored hash to verify against</param>
    /// <returns>True if password matches hash; false otherwise</returns>
    bool VerifyPassword(string plainPassword, string hash);
}
