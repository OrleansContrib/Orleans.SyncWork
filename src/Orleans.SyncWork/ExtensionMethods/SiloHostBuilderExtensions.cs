using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

    /// <summary>
    /// Registers the required <see cref="LimitedConcurrencyLevelTaskScheduler"/>, and scans marker assemblies for the
    /// automatic registration of implementations against <see cref="ISyncWorker{TRequest,TResult}"/>.
    /// </summary>
    /// <param name="builder">The <see cref="ISiloHostBuilder"/> instance.</param>
    /// <param name="maxSyncWorkConcurrency">
    ///     The maximum amount of concurrent work to perform at a time.  
    ///     The CPU cannot be "tapped out" with concurrent work lest Orleans experience timeouts.
    /// </param>
    /// <param name="markers">
    ///     The assemblies to scan to find implementations of <see cref="ISyncWorker{TRequest,TResult}"/> to register.
    /// </param>
    /// <remarks>
    /// 
    ///     A "general" rule of thumb that I've had success with is using "Environment.ProcessorCount - 2" as the max concurrency.
    /// 
    /// </remarks>
    /// <returns>The <see cref="ISiloHostBuilder"/> to allow additional fluent API chaining.</returns>
    public static ISiloHostBuilder ConfigureSyncWorkAbstraction(this ISiloHostBuilder builder,
        int maxSyncWorkConcurrency = 4, params Type[] markers)
    {
        builder.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ISyncWorkAbstractionMarker).Assembly).WithReferences());

        builder.ConfigureServices(services =>
        {
            services.AddSingleton(_ => new LimitedConcurrencyLevelTaskScheduler(maxSyncWorkConcurrency));
        });
        
        foreach (var marker in markers)
        {
            var assembly = marker.Assembly;
            var grainImplementations = assembly.ExportedTypes
                .Where(type =>
                {
                    var genericInterfaceTypes = type.GetInterfaces().Where(x => x.IsGenericType).ToList();
                    var implementationSyncWorkType = genericInterfaceTypes
                        .Any(x => x.GetGenericTypeDefinition() == typeof(ISyncWorker<,>));

                    return !type.IsInterface && !type.IsAbstract && implementationSyncWorkType;
                }).ToList();

            var serviceDescriptors = grainImplementations.Select(grainImplementation =>
                new ServiceDescriptor(grainImplementation, grainImplementation, ServiceLifetime.Transient));
            
            builder.ConfigureServices(x => x.TryAdd(serviceDescriptors));
        }
        
        return builder;
    }
}
