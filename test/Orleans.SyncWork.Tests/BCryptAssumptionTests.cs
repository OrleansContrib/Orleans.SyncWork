using FluentAssertions;
using Orleans.SyncWork.Demo.Services;
using Xunit;

namespace Orleans.SyncWork.Tests;

public class BCryptAssumptionTests
{
    [Fact]
    public void WhenGivenCorrectPasswordAndHash_ShouldReturnTrue()
    {
        var verifyResult = BCrypt.Net.BCrypt.Verify(PasswordConstants.Password, PasswordConstants.PasswordHash);

        verifyResult.Should().BeTrue();
    }

    [Fact]
    public void WhenGivenIncorrectPasswordAndHash_ShouldReturnFalse()
    {
        var mangledPassword = PasswordConstants.Password + "doots ";

        var verifyResult = BCrypt.Net.BCrypt.Verify(mangledPassword, PasswordConstants.PasswordHash);

        verifyResult.Should().BeFalse();
    }
}
