using Xunit;

namespace Orleans.SyncWork.Tests.TestClusters;

[CollectionDefinition(Name)]
public class ClusterCollection : ICollectionFixture<ClusterFixture>
{
    public const string Name = "ClusterCollection";
}
