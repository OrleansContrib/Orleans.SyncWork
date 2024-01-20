using System;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.SyncWork.Demo.Services;
using Orleans.SyncWork.ExtensionMethods;
using Orleans.TestingHost;

namespace Orleans.SyncWork.Tests.TestClusters;

/// <summary>
/// Fixture for creating and eventually disposing a <see cref="TestCluster"/> for use in testing.
/// </summary>
public class ClusterFixture : IDisposable
{
    private class TestSiloConfigurations : ISiloConfigurator
    {
        public void Configure(ISiloBuilder siloBuilder)
        {
            siloBuilder.ConfigureSyncWorkAbstraction(GetMaxConcurrentGrainWork());
            siloBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<IPasswordVerifier, PasswordVerifier>();
            });
        }

        /// <summary>
        /// CI build server is quite resource constrained, hoping doing a "minimum of 1" concurrency will still work.
        /// </summary>
        /// <returns>The max concurrency to register to the <see cref="LimitedConcurrencyLevelTaskScheduler"/>.</returns>
        private static int GetMaxConcurrentGrainWork()
        {
            var concurrentWork = Environment.ProcessorCount - 4;
            
            if (concurrentWork <= 0)
                concurrentWork = 1;

            return concurrentWork;
        }
    }

    public ClusterFixture()
    {
        var builder = new TestClusterBuilder();

        builder.AddSiloBuilderConfigurator<TestSiloConfigurations>();

        Cluster = builder.Build();
        Cluster.Deploy();
    }

    public void Dispose()
    {
        Cluster.StopAllSilos();
    }

    public TestCluster Cluster { get; }
}
