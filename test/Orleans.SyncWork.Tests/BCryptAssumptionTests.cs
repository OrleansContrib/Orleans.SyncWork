using FluentAssertions;
using Orleans.SyncWork.Demo.Api.Services;
using Xunit;

namespace Orleans.SyncWork.Tests;

public class BCryptAssumptionTests
{
    [Fact]
    public void WhenGivenCorrectPasswordAndHash_ShouldReturnTrue()
    {
        var verifyResult = BCrypt.Net.BCrypt.Verify(IPasswordVerifier.Password, IPasswordVerifier.PasswordHash);

        verifyResult.Should().BeTrue();
    }

    [Fact]
    public void WhenGivenIncorrectPasswordAndHash_ShouldReturnFalse()
    {
        var mangledPassword = IPasswordVerifier.Password + "doots ";

        var verifyResult = BCrypt.Net.BCrypt.Verify(mangledPassword, IPasswordVerifier.PasswordHash);

        verifyResult.Should().BeFalse();
    }
}
