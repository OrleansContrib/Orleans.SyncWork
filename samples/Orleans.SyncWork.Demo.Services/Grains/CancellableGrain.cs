using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Orleans.SyncWork.Demo.Services.Grains;

public class CancellableGrain : SyncWorker<SampleCancellationRequest, SampleCancellationResult>
{
    public CancellableGrain(
        ILogger<CancellableGrain> logger,
        LimitedConcurrencyLevelTaskScheduler limitedConcurrencyScheduler) : base(logger, limitedConcurrencyScheduler)
    {
    }

    protected override async Task<SampleCancellationResult> PerformWork(
        SampleCancellationRequest request, GrainCancellationToken grainCancellationToken)
    {
        var startingValue = request.StartingValue;

        for (var i = 0; i < request.EnumerationMax; i++)
        {
            if (grainCancellationToken.CancellationToken.IsCancellationRequested)
            {
                Logger.LogInformation("Task cancelled on iteration {Iteration}", i);

                if (request.ThrowOnCancel)
                    throw new OperationCanceledException(grainCancellationToken.CancellationToken);
                
                return new SampleCancellationResult() {EndingValue = startingValue};
            }

            startingValue += 1;
            await Task.Delay(request.EnumerationDelay);
        }

        return new SampleCancellationResult() { EndingValue = startingValue };
    }
}

[GenerateSerializer]
public class SampleCancellationRequest
{
    [Id(0)]
    public TimeSpan EnumerationDelay { get; init; }
    [Id(1)]
    public int StartingValue { get; init; }
    [Id(2)]
    public int EnumerationMax { get; init; } = 1_000;
    [Id(3)] 
    public bool ThrowOnCancel { get; init; }
}

[GenerateSerializer]
public class SampleCancellationResult
{
    [Id(0)]
    public int EndingValue { get; init; }
}
