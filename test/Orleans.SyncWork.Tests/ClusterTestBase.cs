using Orleans.TestingHost;
using Xunit;

namespace Orleans.SyncWork.Tests;

[Collection(ClusterCollection.Name)]
public abstract class ClusterTestBase
{
    protected readonly TestCluster _cluster;

    public ClusterTestBase(ClusterFixture fixture)
    {
        _cluster = fixture.Cluster;
    }
}
