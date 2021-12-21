using System.Threading.Tasks;

namespace Orleans.SyncWork.Tests.TestGrains;

public interface ITestGrain : IGrain, IGrainWithGuidKey
{
    Task<string> Get();
}

public class TestGrain : Grain, ITestGrain
{
    public Task<string> Get()
    {
        return Task.FromResult(
            "This is a test grain so that the test project has a grain that can be resolved (that isn't a sync-work grain) when setting up a test cluster");
    }
}
