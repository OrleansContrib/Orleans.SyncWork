using System.Threading.Tasks;

namespace Orleans.SyncWork.Demo.Services;

/// <summary>
/// Represents the contact for verifying a password.
/// </summary>
public interface IPasswordVerifier
{
    /// <summary>
    /// Exposes a means of verifying a password hash against a password
    /// </summary>
    /// <param name="passwordHash">The persisted password hash.</param>
    /// <param name="password">The raw password to hash, then compare.</param>
    /// <returns>True when the password verified successfully, false otherwise.</returns>
    Task<bool> VerifyPassword(string? passwordHash, string? password);
}
