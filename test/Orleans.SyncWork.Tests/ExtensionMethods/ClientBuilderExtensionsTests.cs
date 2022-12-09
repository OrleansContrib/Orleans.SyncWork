using System;
using System.Threading.Tasks;
using FluentAssertions;
using Orleans.SyncWork.Demo.Services.Grains;
using Orleans.TestingHost;
using Xunit;

namespace Orleans.SyncWork.Tests.ExtensionMethods;

public class ClientBuilderExtensionsTests
{
    [Fact]
    public async Task WhenNotCallingConfigureSyncWorkAbstraction_ShouldNotResolveSyncWorkGrain()
    {
        var builder = new TestClusterBuilder();

        var cluster = builder.Build();
        await cluster.DeployAsync();

        var action = new Action(() => cluster.Client.GetGrain<ISyncWorker<PasswordVerifierRequest, PasswordVerifierResult>>(Guid.NewGuid()));

        action.Should().Throw<InvalidOperationException>("The extension method to add parts was not invoked, and an exception will be thrown as no grain reference can be made.");
    }

    [Fact]
    public async Task WhenCallingConfigureSyncWorkAbstraction_ShouldResolveSyncWorkGrain()
    {
        var builder = new TestClusterBuilder();

        var cluster = builder.Build();
        await cluster.DeployAsync();
        var grain = cluster.Client.GetGrain<ISyncWorker<PasswordVerifierRequest, PasswordVerifierResult>>(Guid.NewGuid());

        grain.Should().NotBeNull("The extension method to add parts was invoked");
    }
}
