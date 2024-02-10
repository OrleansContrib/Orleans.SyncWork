using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Orleans.SyncWork.Demo.Services.TestGrains;

public class GrainThatWaitsSetTimePriorToExceptionResultBecomingAvailable :
    SyncWorker<TestDelayExceptionRequest, TestDelayExceptionResult>,
    IGrainThatWaitsSetTimePriorToExceptionResultBecomingAvailable
{
    public GrainThatWaitsSetTimePriorToExceptionResultBecomingAvailable(
        ILogger<GrainThatWaitsSetTimePriorToExceptionResultBecomingAvailable> logger,
        LimitedConcurrencyLevelTaskScheduler limitedConcurrencyScheduler
    ) : base(logger, limitedConcurrencyScheduler)
    {
    }

    protected override async Task<TestDelayExceptionResult> PerformWork(TestDelayExceptionRequest request,
        GrainCancellationToken grainCancellationToken)
    {
        Logger.LogInformation($"Waiting {request.MsDelayPriorToResult} on {this.IdentityString}");
        await Task.Delay(request.MsDelayPriorToResult);

        throw new TestGrainException("This is an expected exception, I'm testing for it!");
    }
}

[GenerateSerializer]
public class TestGrainException : Exception
{
    public TestGrainException(string message) : base(message) { }
}

[GenerateSerializer]
public class TestDelayExceptionRequest
{
    [Id(0)]
    public int MsDelayPriorToResult { get; set; }
}

[GenerateSerializer]
public class TestDelayExceptionResult;
