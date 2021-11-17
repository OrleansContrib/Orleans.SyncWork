using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.SyncWork.Demo.Api.Services;
using Orleans.SyncWork.Demo.Api.Services.Grains;
using Orleans.SyncWork.ExtensionMethods;

namespace Orleans.SyncWork.Demo.Api
{
    public static class OrleansConfigurationHelper
    {
        public static async Task<ISiloHost> StartSilo()
        {
            // define the cluster configuration
            var builder = new SiloHostBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "HelloWorldApp";
                })
                .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IHelloWorld).Assembly).WithReferences())
                .ConfigureSyncWorkAbstraction()
                .ConfigureLogging(logging => logging.AddConsole());

            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IHelloWorld, HelloWorld>();
                services.AddSingleton<IPasswordVerifier, Services.PasswordVerifier>();
                services.AddSingleton<ISyncWorker<PasswordVerifierRequest, PasswordVerifierResponse>, Services.Grains.PasswordVerifier>();
            });

            var host = builder.Build();
            await host.StartAsync();
            return host;
        }
    }
}
