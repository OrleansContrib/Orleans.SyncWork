using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Orleans.SyncWork.Demo.Services.Grains;
using Orleans.SyncWork.ExtensionMethods;
using Orleans.SyncWork.Tests.TestGrains;
using Orleans.TestingHost;
using Xunit;

namespace Orleans.SyncWork.Tests.ExtensionMethods;

public class ClientBuilderExtensionsTests
{
    private class ClientBuilderNoConfigureSyncWorkAbstraction : IClientBuilderConfigurator
    {
        public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
        {
            // Configuring the addition of application parts for *this* assembly only, so that the automatic assembly scanning,
            // which would pick up Orleans.SyncWork grains does not fire.
            clientBuilder.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(TestGrain).Assembly));
        }
    }
    
    private class ClientBuilderConfigureSyncWorkAbstraction : IClientBuilderConfigurator
    {
        public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
        {
            clientBuilder.ConfigureSyncWorkAbstraction();
        }
    }
    
    [Fact]
    public async Task WhenNotCallingConfigureSyncWorkAbstraction_ShouldNotResolveSyncWorkGrain()
    {
        var builder = new TestClusterBuilder();
        builder.AddClientBuilderConfigurator<ClientBuilderNoConfigureSyncWorkAbstraction>();
        
        var cluster = builder.Build();
        await cluster.DeployAsync();

        var action = new Action(() => cluster.Client.GetGrain<ISyncWorker<PasswordVerifierRequest, PasswordVerifierResult>>(Guid.NewGuid()));
        
        action.Should().Throw<InvalidOperationException>("The extension method to add parts was not invoked, and an exception will be thrown as no grain reference can be made.");
    }
    
    [Fact]
    public async Task WhenCallingConfigureSyncWorkAbstraction_ShouldResolveSyncWorkGrain()
    {
        var builder = new TestClusterBuilder();
        builder.AddClientBuilderConfigurator<ClientBuilderConfigureSyncWorkAbstraction>();
        
        var cluster = builder.Build();
        await cluster.DeployAsync();
        var grain = cluster.Client.GetGrain<ISyncWorker<PasswordVerifierRequest, PasswordVerifierResult>>(Guid.NewGuid());

        grain.Should().NotBeNull("The extension method to add parts was invoked");
    }
}
