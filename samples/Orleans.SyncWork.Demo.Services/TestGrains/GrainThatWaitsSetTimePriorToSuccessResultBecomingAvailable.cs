using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Orleans.SyncWork.Demo.Services.TestGrains;

[GenerateSerializer]
public class TestDelaySuccessRequest
{
    [Id(0)]
    public DateTime Started { get; set; }
    [Id(1)]
    public int MsDelayPriorToResult { get; set; }
}

[GenerateSerializer]
public class TestDelaySuccessResult
{
    [Id(0)]
    public DateTime Started { get; set; }
    [Id(1)]
    public DateTime Ended { get; set; }
}

public class GrainThatWaitsSetTimePriorToSuccessResultBecomingAvailable : SyncWorker<TestDelaySuccessRequest, TestDelaySuccessResult>
{
    public GrainThatWaitsSetTimePriorToSuccessResultBecomingAvailable(
        ILogger<GrainThatWaitsSetTimePriorToSuccessResultBecomingAvailable> logger,
        LimitedConcurrencyLevelTaskScheduler limitedConcurrencyScheduler
    ) : base(logger, limitedConcurrencyScheduler) { }

    protected override async Task<TestDelaySuccessResult> PerformWork(TestDelaySuccessRequest request)
    {
        await Task.Delay(request.MsDelayPriorToResult);

        return new TestDelaySuccessResult()
        {
            Started = request.Started,
            Ended = DateTime.UtcNow
        };
    }
}
