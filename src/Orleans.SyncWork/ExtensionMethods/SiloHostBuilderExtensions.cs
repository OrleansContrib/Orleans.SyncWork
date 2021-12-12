using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;

namespace Orleans.SyncWork.ExtensionMethods;

/// <summary>
/// Extension methods for <see cref="ISiloHostBuilder"/>.
/// </summary>
public static class SiloHostBuilderExtensions
{
    /// <summary>
    /// Configures assembly scanning against this assembly containing the <see cref="ISyncWorker{TRequest, TResult}"/>.
    /// </summary>
    /// <param name="builder">The <see cref="ISiloHostBuilder"/> instance.</param>
    /// <param name="maxSyncWorkConcurrency">
    ///     The maximum amount of concurrent work to perform at a time.  
    ///     The CPU cannot be "tapped out" with concurrent work lest Orleans experience timeouts.
    /// </param>
    /// <remarks>
    /// 
    ///     A "general" rule of thumb that I've had success with is using "Environment.ProcessorCount - 2" as the max concurrency.
    /// 
    /// </remarks>
    /// <returns>The <see cref="ISiloHostBuilder"/> to allow additional fluent API chaining.</returns>
    public static ISiloHostBuilder ConfigureSyncWorkAbstraction(this ISiloHostBuilder builder, int maxSyncWorkConcurrency = 4)
    {
        builder.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ISyncWorkAbstractionMarker).Assembly).WithReferences());

        builder.ConfigureServices(services =>
        {
            services.AddSingleton(_ => new LimitedConcurrencyLevelTaskScheduler(maxSyncWorkConcurrency));
        });

        return builder;
    }
}
