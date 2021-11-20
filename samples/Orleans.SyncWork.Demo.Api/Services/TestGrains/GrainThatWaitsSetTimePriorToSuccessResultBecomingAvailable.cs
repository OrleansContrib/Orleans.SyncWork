using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Orleans.SyncWork.Demo.Api.Services.TestGrains;

public class TestDelaySuccessRequest
{
    public DateTime Started { get; set; }
    public int MsDelayPriorToResult { get; set; }
}

public class TestDelaySuccessResult
{
    public DateTime Started { get; set; }
    public DateTime Ended { get; set; }

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
