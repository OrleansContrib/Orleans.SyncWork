[![Build and test](https://github.com/OrleansContrib/Orleans.SyncWork/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/OrleansContrib/Orleans.SyncWork/actions/workflows/ci.yml) [![Coverage Status](https://coveralls.io/repos/github/OrleansContrib/Orleans.SyncWork/badge.svg?branch=main)](https://coveralls.io/github/OrleansContrib/Orleans.SyncWork?branch=main)

![Latest NuGet Version](https://img.shields.io/nuget/v/Orleans.SyncWork) 
![License](https://img.shields.io/github/license/OrleansContrib/Orleans.SyncWork)

This package's intention is to expose an abstract base class to allow [Orleans](https://github.com/dotnet/orleans/) to work with long running, CPU bound, synchronous work, without becoming overloaded.

Built with an open source <a href="https://jb.gg/OpenSourceSupport"><img src="docs/images/Rider_icon.svg" width=25 height=25></a> license, thanks Jetbrains!

## Project Overview

There are several projects within this repository, all with the idea of demonstrating and/or testing the claim that the NuGet package https://www.nuget.org/packages/Orleans.SyncWork/ does what it is claimed it does.

The projects in this repository include:

* [Orleans.SyncWork](#orleanssyncwork)
* [Orleans.SyncWork.Tests](#orleanssyncworktests)
* [Orleans.SyncWork.Demo.Api](#orleanssyncworkdemoapi)
* [Orleans.SyncWork.Demo.Api.Benchmark](#orleanssyncworkdemoapibenchmark)
* [Orleans.SyncWork.Demo.Services](#orleanssyncworkdemoservices)

### Orleans.SyncWork

The meat and potatoes of the project.  This project contains the abstraction of "Long Running, CPU bound, Synchronous work" in the form of an abstract base class [SyncWorker](https://github.com/OrleansContrib/Orleans.SyncWork/blob/main/src/Orleans.SyncWork/SyncWorker.cs); which implements an interface [ISyncWorker](https://github.com/OrleansContrib/Orleans.SyncWork/blob/main/src/Orleans.SyncWork/ISyncWorker.cs).

When long running work is identified, you can extend the base class `SyncWorker`, providing a `TRequest` and `TResponse` unique to the long running work.  This allows you to create as many `ISyncWork<TRequest, TResponse>` implementations as necessary, for all your long running CPU bound needs! (At least that is the hope.)

Basic "flow" of the SyncWork:

* `Start`
* Poll `GetStatus` until a `Completed` or `Faulted` status is received
* `GetResult` or `GetException` depending on the `GetStatus`

This package introduces a few "requirements" against Orleans:

* In order to not overload Orleans, a `LimitedConcurrencyLevelTaskScheduler` is introduced. This task scheduler is registered (either manually or through the provided extension method) with a maximum level of concurrency for the silo being set up.  This maximum concurrency ***MUST*** allow for idle threads, lest the Orleans server be overloaded.  In testing, the general rule of thumb was `Environment.ProcessorCount - 2` max concurrency.  The important part is that the CPU is not fully "tapped out" such that the normal Orleans asynchronous messaging can't make it through due to the blocking sync work - this will make things start timing out.
* Blocking grains are stateful, and are currently keyed on a Guid.  If in a situation where multiple grains of long running work is needed, each grain should be initialized with its own unique identity.
* Blocking grains *likely* ***CAN NOT*** dispatch further blocking grains.  This is not yet tested under the repository, but it stands to reason that with a limited concurrency scheduler, the following scenario would lead to a deadlock:
    * Grain A is long running
    * Grain B is long running
    * Grain A initializes and fires off Grain B
    * Grain A cannot complete its work until it gets the results of Grain B

    In the above scenario, if "Grain A" is "actively being worked" and it fires off a "Grain B", but "Grain A" cannot complete its work until "Grain B" finishes its own, but "Grain B" cannot *start* its work until "Grain A" finishes its work due to limited concurrency, you've run into a situation where the limited concurrency task scheduler can never finish the work of "Grain A".
    
    That was quite a sentence, hopefully the point was conveyed somewhat sensibly. There may be a way to avoid the above scenario, but I have not yet deeply explored it.

#### Usage

Extend the base class to implement a long running grain.

```cs
public class PasswordVerifier : SyncWorker<PasswordVerifierRequest, PasswordVerifierResult>, IGrain
{
    private readonly IPasswordVerifier _passwordVerifier;

    public PasswordVerifier(
        ILogger<PasswordVerifier> logger, 
        LimitedConcurrencyLevelTaskScheduler limitedConcurrencyLevelTaskScheduler, 
        IPasswordVerifier passwordVerifier) : base(logger, limitedConcurrencyLevelTaskScheduler)
    {
        _passwordVerifier = passwordVerifier;
    }

    protected override async Task<PasswordVerifierResult> PerformWork(PasswordVerifierRequest request)
    {
        var verifyResult = await _passwordVerifier.VerifyPassword(request.PasswordHash, request.Password);

        return new PasswordVerifierResult()
        {
            IsValid = verifyResult
        };
    }
}
public class PasswordVerifierRequest
{
    public string Password { get; set; }
    public string PasswordHash { get; set; }
}

public class PasswordVerifierResult
{
    public bool IsValid { get; set; }
}
```

Run the grain:

```cs
var request = new PasswordVerifierRequest()
{
    Password = "my super neat password that's totally secure because it's super long",
    PasswordHash = "$2a$11$vBzJ4Ewx28C127AG5x3kT.QCCS8ai0l4JLX3VOX3MzHRkF4/A5twy"
}
var passwordVerifyGrain = grainFactory.GetGrain<ISyncWorker<PasswordVerifierRequest, PasswordVerifierResult>>(Guid.NewGuid());
var result = await passwordVerifyGrain.StartWorkAndPollUntilResult(request);
```

The above `StartWorkAndPollUntilResult` is an extension method defined in the package ([SyncWorkerExtensions](https://github.com/OrleansContrib/Orleans.SyncWork/blob/main/src/Orleans.SyncWork/SyncWorkerExtensions.cs)) that `Start`s, `Poll`s, and finally `GetResult` or `GetException` upon completed work.  There would seemingly be place for improvement here as it relates to testing unexpected scenarios, configuration based polling, etc.

### Orleans.SyncWork.Tests

Unit testing project for the work in [Orleans.SyncWork](#orleanssyncwork).  These tests bring up a "TestCluster" which is used for the full duration of the tests against the grains.

One of the tests in particular throws 10k grains onto the cluster at once, all of which are long running (~200ms each) on my machine - more than enough time to overload the cluster if the limited concurrency task scheduler is not working along side the `SyncWork` base class correctly.

TODO: still could use a few more unit tests here to if nothing else, document behavior.

### Orleans.SyncWork.Demo.Api

This is a demo of the `ISyncWork<TRequest, TResult>` in action.  This project is being used as both a Orleans Silo, and client.  Generally you would stand up nodes to the cluster separate from the clients against the cluster.  Since we have only one node for testing purposes, this project acts as both the silo host and client.

The [OrleansDashboard](https://github.com/OrleansContrib/OrleansDashboard) is also brought up with the API.  You can see an example of hitting an endpoint in which 10k password verification requests are received here:

![Dashboard showing 10k CPU bound, long running requests](/docs/images/dashboard.PNG)

Swagger UI is also made available to the API for testing out the endpoints for demo purposes.

### Orleans.SyncWork.Demo.Api.Benchmark

Utilizing [Benchmark DotNet](https://benchmarkdotnet.org/index.html), a benchmarking class was created to both test that the cluster wasn't falling over, and see what sort of timing situation we're dealing with.

Following is the benchmark used at the time of writing:

```cs
public class Benchy
{
    const int TotalNumberPerBenchmark = 100;
    private readonly IPasswordVerifier _passwordVerifier = new Services.PasswordVerifier();
    private readonly PasswordVerifierRequest _request = new PasswordVerifierRequest()
    {
        Password = PasswordConstants.Password,
        PasswordHash = PasswordConstants.PasswordHash
    };

    [Benchmark]
    public void Serial()
    {
        for (var i = 0; i < TotalNumberPerBenchmark; i++)
        {
            _passwordVerifier.VerifyPassword(PasswordConstants.PasswordHash, PasswordConstants.Password);
        }
    }

    [Benchmark]
    public async Task MultipleTasks()
    {
        var tasks = new List<Task>();
        for (var i = 0; i < TotalNumberPerBenchmark; i++)
        {
            tasks.Add(_passwordVerifier.VerifyPassword(PasswordConstants.PasswordHash, PasswordConstants.Password));
        }

        await Task.WhenAll(tasks);
    }

    [Benchmark]
    public async Task MultipleParallelTasks()
    {
        var tasks = new List<Task>();

        Parallel.For(0, TotalNumberPerBenchmark, i =>
        {
            tasks.Add(_passwordVerifier.VerifyPassword(PasswordConstants.PasswordHash, PasswordConstants.Password));
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
```

And here are the results:

|                Method |     Mean |    Error |   StdDev |
|---------------------- |---------:|---------:|---------:|
|                Serial | 12.399 s | 0.0087 s | 0.0077 s |
|         MultipleTasks | 12.289 s | 0.0106 s | 0.0094 s |
| MultipleParallelTasks |  1.749 s | 0.0347 s | 0.0413 s |
|          OrleansTasks |  2.130 s | 0.0055 s | 0.0084 s |

And of course note, that in the above the Orleans tasks are *limited* to my local cluster.  In a more real situation where you have multiple nodes to the cluster, you could expect to get better timing, though you'd probably have to deal more with network latency.

### Orleans.SyncWork.Demo.Services

This project defines several grains to demonstrate the workings of the `Orleans.SyncWork` package, through the Web API, benchmark, and tests.
