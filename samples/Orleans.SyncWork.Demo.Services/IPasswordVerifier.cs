using System.Threading.Tasks;

namespace Orleans.SyncWork.Demo.Services;

/// <summary>
/// Represents the contact for verifying a password.
/// </summary>
public interface IPasswordVerifier
{
    public const string Password = "my super neat password that's totally secure because it's super long";
    public const string PasswordHash = "$2a$11$vBzJ4Ewx28C127AG5x3kT.QCCS8ai0l4JLX3VOX3MzHRkF4/A5twy";

    /// <summary>
    /// Exposes a means of verifying a password hash against a password
    /// </summary>
    /// <param name="passwordHash">The persisted password hash.</param>
    /// <param name="password">The raw password to hash, then compare.</param>
    /// <returns>True when the password verified successfully, false otherwise.</returns>
    Task<bool> VerifyPassword(string passwordHash, string password);
}
