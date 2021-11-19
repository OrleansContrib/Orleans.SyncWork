using System;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.SyncWork.Demo.Api.Services;
using Orleans.TestingHost;

namespace Orleans.SyncWork.Tests;

public class ClusterFixture : IDisposable
{
    public class TestSiloConfigurations : ISiloConfigurator
    {
        public void Configure(ISiloBuilder siloBuilder)
        {
            siloBuilder.ConfigureServices(services => {
                services.AddSingleton<IPasswordVerifier, PasswordVerifier>();
                var limitedConcurrencyLevelTaskScheduler = new LimitedConcurrencyLevelTaskScheduler(Environment.ProcessorCount - 2);
                services.AddSingleton(limitedConcurrencyLevelTaskScheduler);
            });
        }
    }

    public ClusterFixture()
    {
        var builder = new TestClusterBuilder();

        // TODO is there no way to just use the `ISiloHostBuilder` configuration being built for the *actual* silo?
        // Why is it using a `ISiloBuilder` rather than `ISiloHostBuilder`?
        builder.AddSiloBuilderConfigurator<TestSiloConfigurations>();

        Cluster = builder.Build();
        Cluster.Deploy();
    }

    public void Dispose()
    {
        Cluster.StopAllSilos();
    }

    public TestCluster Cluster { get; private set; }
}
