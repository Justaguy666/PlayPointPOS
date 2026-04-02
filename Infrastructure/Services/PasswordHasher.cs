using Application.Services;
using BC = BCrypt.Net.BCrypt;

namespace Infrastructure.Services;

/// <summary>
/// Password hasher using BCrypt algorithm.
/// BCrypt is designed for secure password hashing with built-in salt and work factor.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    /// <summary>
    /// Hashes a plain-text password using BCrypt with work factor 12.
    /// Work factor 12 provides good security (as of 2024) while maintaining reasonable performance.
    /// </summary>
    public string HashPassword(string plainPassword)
    {
        // BCrypt.Net-Next with work factor 12: ~100-150ms on modern hardware
        // Higher = more secure but slower; lower = faster but less secure
        // 12 is recommended as of 2024
        return BC.HashPassword(plainPassword, workFactor: 12);
    }

    /// <summary>
    /// Verifies a plain-text password against a BCrypt hash.
    /// Falls back to plain-text comparison for migration from old plain-text passwords.
    /// </summary>
    public bool VerifyPassword(string plainPassword, string hash)
    {
        try
        {
            // Try BCrypt verification (for hashed passwords)
            return BC.Verify(plainPassword, hash);
        }
        catch
        {
            // Fallback: support old plain-text hashes during migration period
            // Remove this fallback after all passwords are migrated to BCrypt
            return plainPassword == hash;
        }
    }
}

