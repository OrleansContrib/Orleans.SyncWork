using Orleans.TestingHost;
using Xunit;

namespace Orleans.SyncWork.Tests.TestClusters;

/// <summary>
/// An abstract class containing a <see cref="TestCluster"/> for use in tests against
/// the Orleans grains.
/// </summary>
[Collection(ClusterCollection.Name)]
public abstract class ClusterTestBase
{
    protected readonly TestCluster Cluster;

    protected ClusterTestBase(ClusterFixture fixture)
    {
        Cluster = fixture.Cluster;
    }
}
