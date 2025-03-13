using System;
using System.Threading.Tasks;
using BCrypt.Net;
using FluentAssertions;
using Orleans.SyncWork.Demo.Services;
using Orleans.SyncWork.Demo.Services.Grains;
using Orleans.SyncWork.Exceptions;
using Orleans.SyncWork.Tests.TestClusters;
using Xunit;

namespace Orleans.SyncWork.Tests;

/// <summary>
/// This test class is not *necessarily* specific the the <see cref="Demo.Services.PasswordVerifier"/> grain.
/// It is more intended to demonstrate the workings of the "flow" of using the <see cref="ISyncWorker{TRequest, TResult}"/>
/// as far as getting expected results and exceptions from the execution of the grain using the extension method(s)
/// in <see cref="SyncWorkerExtensions"/>.
/// </summary>
public class SyncWorkerExtensionMethodTests : ClusterTestBase
{
    public SyncWorkerExtensionMethodTests(ClusterFixture fixture) : base(fixture) { }

    [Fact]
    public async Task WhenGivenValidPasswordAndHash_ShouldVerify()
    {
        var grain = Cluster.GrainFactory.GetGrain<IPasswordVerifierGrain>(Guid.NewGuid());

        var request = new PasswordVerifierRequest
        {
            Password = PasswordConstants.Password,
            PasswordHash = PasswordConstants.PasswordHash
        };

        var result = await grain.StartWorkAndPollUntilResult(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task WhenGivenInvalidPasswordAndHash_ShouldNotVerify()
    {
        var grain = Cluster.GrainFactory.GetGrain<IPasswordVerifierGrain>(Guid.NewGuid());

        var request = new PasswordVerifierRequest
        {
            Password = PasswordConstants.Password + "doot",
            PasswordHash = PasswordConstants.PasswordHash
        };

        var result = await grain.StartWorkAndPollUntilResult(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task WhenGivenBadHashFormat_ShouldException()
    {
        var grain = Cluster.GrainFactory.GetGrain<IPasswordVerifierGrain>(Guid.NewGuid());

        var request = new PasswordVerifierRequest
        {
            Password = PasswordConstants.Password,
            PasswordHash = "this is an invalid hash"
        };

        Func<Task> action = async () => await grain.StartWorkAndPollUntilResult(request);

        await action.Should().ThrowAsync<GrainFaultedException>()
            .WithInnerException<GrainFaultedException, SaltParseException>();
    }
}

