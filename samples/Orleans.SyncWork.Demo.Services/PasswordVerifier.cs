using System.Threading.Tasks;

namespace Orleans.SyncWork.Demo.Services;

public class PasswordVerifier : IPasswordVerifier
{
    /// <summary>
    /// A "base" implementation of verify password, no concurrency through orleans, etc.
    /// </summary>
    /// <inheritdoc/>
    public Task<bool> VerifyPassword(string? passwordHash, string? password)
    {
        return Task.FromResult(BCrypt.Net.BCrypt.Verify(password, passwordHash));
    }
}
