using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.SyncWork.Demo.Services;
using Orleans.SyncWork.ExtensionMethods;

Console.WriteLine("Hello, World!");

IHostBuilder builder = Host.CreateDefaultBuilder(args)
    .UseOrleans(silo =>
    {
        silo.UseLocalhostClustering()
            .ConfigureLogging(logging => logging.AddConsole());
        silo.ConfigureSyncWorkAbstraction(2);
    })
    .UseConsoleLifetime()
    .ConfigureServices(collection =>
    {
        collection.AddSingleton<IPasswordVerifier, PasswordVerifier>();
    });

using IHost host = builder.Build();

await host.RunAsync();
