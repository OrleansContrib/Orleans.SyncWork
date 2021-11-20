using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Orleans.SyncWork.Demo.Api.Services;
using Orleans.SyncWork.Demo.Api.Services.Grains;

namespace Orleans.SyncWork.Demo.Api.Benchmark;

public class Benchy
{
    const int TotalNumberPerBenchmark = 100;
    private readonly IPasswordVerifier _passwordVerifier = new Services.PasswordVerifier();
    private readonly PasswordVerifierRequest _request = new PasswordVerifierRequest()
    {
        Password = IPasswordVerifier.Password,
        PasswordHash = IPasswordVerifier.PasswordHash
    };

    [Benchmark]
    public void Serial()
    {
        for (var i = 0; i < TotalNumberPerBenchmark; i++)
        {
            _passwordVerifier.VerifyPassword(IPasswordVerifier.PasswordHash, IPasswordVerifier.Password);
        }
    }

    [Benchmark]
    public async Task MultipleTasks()
    {
        var tasks = new List<Task>();
        for (var i = 0; i < TotalNumberPerBenchmark; i++)
        {
            tasks.Add(_passwordVerifier.VerifyPassword(IPasswordVerifier.PasswordHash, IPasswordVerifier.Password));
        }

        await Task.WhenAll(tasks);
    }

    [Benchmark]
    public async Task MultipleParallelTasks()
    {
        var tasks = new List<Task>();

        Parallel.For(0, TotalNumberPerBenchmark, i =>
        {
            tasks.Add(_passwordVerifier.VerifyPassword(IPasswordVerifier.PasswordHash, IPasswordVerifier.Password));
        });

        await Task.WhenAll(tasks);
    }

    [Benchmark]
    public async Task OrleansTasks()
    {
        var siloHost = await BenchmarkingSIloHost.GetSiloHost();
        var grainFactory = siloHost.Services.GetRequiredService<IGrainFactory>();
        var tasks = new List<Task>();
        for (var i = 0; i < TotalNumberPerBenchmark; i++)
        {
            var grain = grainFactory.GetGrain<ISyncWorker<PasswordVerifierRequest, PasswordVerifierResult>>(Guid.NewGuid());
            tasks.Add(grain.StartWorkAndPollUntilResult(_request));
        }

        await Task.WhenAll(tasks);
    }
}
