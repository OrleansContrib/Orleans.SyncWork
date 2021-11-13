namespace Orleans.SyncWork.Demo.Api.Services;

public class PasswordHasher : IPasswordHasher
{
    /// <summary>
    /// A "base" implementation of hash password, no concurrency through orleans, etc.
    /// </summary>
    /// <param name="password">The password to hash</param>
    /// <returns></returns>
    public Task<string> HashPassword(string password)
    {
        return Task.FromResult(BCrypt.Net.BCrypt.HashPassword(password, 10));
    }
}
