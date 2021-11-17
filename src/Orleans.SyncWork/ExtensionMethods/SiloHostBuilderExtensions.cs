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
    /// <returns>The <see cref="ISiloHostBuilder"/> to allow additional fluent API chaining.</returns>
    public static ISiloHostBuilder ConfigureSyncWorkAbstraction(this ISiloHostBuilder builder)
    {
        builder.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ISyncWorkAbstractionMarker).Assembly).WithReferences());

        return builder;
    }
}
