using Orleans.SyncWork.Tests.TestClusters;
using Orleans.TestingHost;

namespace Orleans.SyncWork.Demo.Api.Benchmark;

internal static class BenchmarkingSiloHost
{
    static TestCluster? _testCluster;
    public static TestCluster GetTestCluster()
    {
        if (_testCluster == null)
        {
            var clusterFixture = new ClusterFixture();
            _testCluster = clusterFixture.Cluster;
        }

        return _testCluster;
    }
}
