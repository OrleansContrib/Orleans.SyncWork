using Orleans.TestingHost;
using Xunit;

namespace Orleans.SyncWork.Tests;

/// <summary>
/// An abstract class containing a <see cref="TestCluster"/> for use in tests against
/// the Orleans grains.
/// </summary>
[Collection(ClusterCollection.Name)]
public abstract class ClusterTestBase
{
    protected readonly TestCluster _cluster;

    public ClusterTestBase(ClusterFixture fixture)
    {
        _cluster = fixture.Cluster;
    }
}
