﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.SyncWork;
using Orleans.SyncWork.Demo.Services;
using Orleans.SyncWork.Demo.Services.Grains;

Console.WriteLine("Hello, World!");

IHostBuilder builder = Host.CreateDefaultBuilder(args)
    .UseOrleansClient(client =>
    {
        client.UseLocalhostClustering();
    })
    .ConfigureLogging(logging => logging.AddConsole())
    .UseConsoleLifetime();

using IHost host = builder.Build();
await host.StartAsync();

IClusterClient client = host.Services.GetRequiredService<IClusterClient>();

IPasswordVerifierGrain grain = client.GetGrain<IPasswordVerifierGrain>(Guid.NewGuid());

var result = await grain.StartWorkAndPollUntilResult(
    new PasswordVerifierRequest()
    {
        Password = PasswordConstants.Password,
        PasswordHash = PasswordConstants.PasswordHash,
    });

Console.WriteLine(
$"""
IsValid password: { result.IsValid }

Press any key to exit...
""");

Console.ReadKey();

await host.StopAsync();
