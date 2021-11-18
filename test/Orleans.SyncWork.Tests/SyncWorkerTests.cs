using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Orleans.SyncWork.Demo.Api.Services;
using Orleans.SyncWork.Demo.Api.Services.Grains;
using Xunit;

namespace Orleans.SyncWork.Tests;

public class SyncWorkerTests : ClusterTestBase
{
    public SyncWorkerTests(ClusterFixture fixture) : base(fixture) { }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(1000)]
    [InlineData(10000)]
    public async Task WhenGivenLargeNumberOfRequests_SystemShouldNotBecomeOverloaded(int totalInvokes)
    {
        var tasks = new List<Task<PasswordVerifierResponse>>();
        var request = new PasswordVerifierRequest
        {
            Password = IPasswordVerifier.Password,
            PasswordHash = IPasswordVerifier.PasswordHash
        };
        for (var i = 0; i < totalInvokes; i++)
        {
            var grain = _cluster.GrainFactory.GetGrain<ISyncWorker<PasswordVerifierRequest, PasswordVerifierResponse>>(Guid.NewGuid());
            tasks.Add(grain.StartWorkAndPollUntilResult(request));
        }

        await Task.WhenAll(tasks);

        tasks.Select(task => task.Result).Should().OnlyContain(result => true);
    }
}
