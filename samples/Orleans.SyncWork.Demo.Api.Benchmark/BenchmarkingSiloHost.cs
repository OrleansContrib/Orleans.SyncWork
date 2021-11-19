using System.Threading.Tasks;
using Orleans.Hosting;

namespace Orleans.SyncWork.Demo.Api.Benchmark;

internal static class BenchmarkingSIloHost
{
    static ISiloHost _siloHost;
    public static async Task<ISiloHost> GetSiloHost()
    {
        if (_siloHost == null)
            _siloHost = await OrleansConfigurationHelper.StartSilo();

        return _siloHost;
    }
    
}
