using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Orleans.SyncWork.Demo.Services.TestGrains;

public class TestDelaySuccessRequest
{
    public DateTime Started { get; init; }
    public int MsDelayPriorToResult { get; init; }
}

public class TestDelaySuccessResult
{
    public DateTime Started { get; init; }
    public DateTime Ended { get; init; }

    public TimeSpan Duration => Ended - Started;
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
