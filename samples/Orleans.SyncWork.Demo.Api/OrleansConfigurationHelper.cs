using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.SyncWork.Demo.Services;
using Orleans.SyncWork.ExtensionMethods;
using PasswordVerifier = Orleans.SyncWork.Demo.Services.PasswordVerifier;

namespace Orleans.SyncWork.Demo.Api
{
    public static class OrleansConfigurationHelper
    {
        /// <summary>
        /// Starts a <see cref="ISiloHost"/> with default-ish options.
        /// </summary>
        /// <returns>The started <see cref="ISiloHost"/>.</returns>
        public static ConfigureHostBuilder ConfigureOrleans(this ConfigureHostBuilder builder, int maxSyncWorkConcurrency)
        {
            builder.UseOrleans((siloBuilder) =>
            {
                siloBuilder
                    .UseLocalhostClustering()
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "dev";
                        options.ServiceId = "HelloWorldApp";
                    })
                    .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                    .ConfigureSyncWorkAbstraction(maxSyncWorkConcurrency)
                    .ConfigureLogging(logging => logging.AddConsole())
                    .ConfigureServices(collection =>
                    {
                        collection.AddSingleton<IPasswordVerifier, PasswordVerifier>();
                    })
                    .UseDashboard(config =>
                    {
                        config.Port = 8081;
                    });
            });

            return builder;
        }
    }
}
