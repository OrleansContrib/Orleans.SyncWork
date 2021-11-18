using System;
using System.Threading.Tasks;
using BCrypt.Net;
using FluentAssertions;
using Orleans.SyncWork.Demo.Api.Services;
using Orleans.SyncWork.Demo.Api.Services.Grains;
using Xunit;

namespace Orleans.SyncWork.Tests;

public class PasswordVerifierTests : ClusterTestBase
{
    public PasswordVerifierTests(ClusterFixture fixture) : base(fixture) { }

    [Fact]
    public async Task WhenGivenValidPasswordAndHash_ShouldVerify()
    {
        var grain = _cluster.GrainFactory.GetGrain<ISyncWorker<PasswordVerifierRequest, PasswordVerifierResponse>>(Guid.NewGuid());

        var request = new PasswordVerifierRequest
        {
            Password = IPasswordVerifier.Password,
            PasswordHash = IPasswordVerifier.PasswordHash
        };

        var result = await grain.StartWorkAndPollUntilResult(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task WhenGivenInvalidPasswordAndHash_ShouldNotVerify()
    {
        var grain = _cluster.GrainFactory.GetGrain<ISyncWorker<PasswordVerifierRequest, PasswordVerifierResponse>>(Guid.NewGuid());

        var request = new PasswordVerifierRequest
        {
            Password = IPasswordVerifier.Password + "doot",
            PasswordHash = IPasswordVerifier.PasswordHash
        };

        var result = await grain.StartWorkAndPollUntilResult(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task WhenGivenBadHashFormat_ShouldExpection()
    {
        var grain = _cluster.GrainFactory.GetGrain<ISyncWorker<PasswordVerifierRequest, PasswordVerifierResponse>>(Guid.NewGuid());

        var request = new PasswordVerifierRequest
        {
            Password = IPasswordVerifier.Password,
            PasswordHash = "this is an invalid hash"
        };

        Func<Task> action = async () => await grain.StartWorkAndPollUntilResult(request);

        await action.Should().ThrowAsync<SaltParseException>();
    }
}

