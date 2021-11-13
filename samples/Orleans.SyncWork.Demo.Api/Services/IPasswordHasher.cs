namespace Orleans.SyncWork.Demo.Api.Services;

/// <summary>
/// Represents the contact for generating a hash from a string password
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes the provided password, and returns it.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>The hashed password</returns>
    Task<string> HashPassword(string password);
}
