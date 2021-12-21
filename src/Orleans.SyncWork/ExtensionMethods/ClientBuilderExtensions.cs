namespace Orleans.SyncWork.ExtensionMethods;

/// <summary>
/// Extension methods for <see cref="IClientBuilder"/>.
/// </summary>
public static class ClientBuilderExtensions
{
    /// <summary>
    /// Configures assembly scanning against this assembly containing the <see cref="ISyncWorker{TRequest, TResult}"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IClientBuilder"/> instance.</param>
    /// <returns>The <see cref="IClientBuilder"/> to allow additional fluent API chaining.</returns>
    public static IClientBuilder ConfigureSyncWorkAbstraction(this IClientBuilder builder)
    {
        builder.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ISyncWorkAbstractionMarker).Assembly).WithReferences());

        return builder;
    }
}
