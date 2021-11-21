using System;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.SyncWork.Demo.Api.Services;
using Orleans.TestingHost;

namespace Orleans.SyncWork.Tests;

/// <summary>
/// Fixture for creating and eventually disposing a <see cref="TestCluster"/> for use in testing.
/// </summary>
public class ClusterFixture : IDisposable
{
    public class TestSiloConfigurations : ISiloConfigurator
    {
        public void Configure(ISiloBuilder siloBuilder)
        {
            siloBuilder.ConfigureServices(services => {
                services.AddSingleton<IPasswordVerifier, PasswordVerifier>();
                var limitedConcurrencyLevelTaskScheduler = new LimitedConcurrencyLevelTaskScheduler(GetMaxConcurrentGrainWork());
                services.AddSingleton(limitedConcurrencyLevelTaskScheduler);
            });
        }

        /// <summary>
        /// CI build server is quite resource constrained, hoping doing a "minimum of 1" concurrency will still work.
        /// </summary>
        /// <returns>The max concurrency to register to the <see cref="LimitedConcurrencyLevelTaskScheduler"/>.</returns>
        private static int GetMaxConcurrentGrainWork()
        {
            var concurrentWork = Environment.ProcessorCount - 2;
            if (concurrentWork <= 0)
                concurrentWork = 1;
            
            return concurrentWork;
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
