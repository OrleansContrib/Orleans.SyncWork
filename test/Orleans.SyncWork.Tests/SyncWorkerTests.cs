using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Orleans.SyncWork.Demo.Services;
using Orleans.SyncWork.Demo.Services.Grains;
using Orleans.SyncWork.Demo.Services.TestGrains;
using Orleans.SyncWork.Enums;
using Orleans.SyncWork.Exceptions;
using Orleans.SyncWork.Tests.TestClusters;
using Orleans.SyncWork.Tests.XUnitTraits;
using Xunit;

namespace Orleans.SyncWork.Tests;

/// <summary>
/// Test against the functionality of the <see cref="SyncWorker{TRequest, TResult}"/> base class.
/// </summary>
public class SyncWorkerTests : ClusterTestBase
{
    private readonly GrainCancellationTokenSource _cancellationTokenSource = new();

    public SyncWorkerTests(ClusterFixture fixture) : base(fixture) { }

    [Theory, Trait(Traits.Category, Traits.Categories.LongRunning)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(1000)]
    public async Task WhenGivenNumberOfRequests_SystemShouldNotBecomeOverloaded(int totalInvokes)
    {
        var tasks = new List<Task<PasswordVerifierResult>>();
        var request = new PasswordVerifierRequest
        {
            Password = PasswordConstants.Password,
            PasswordHash = PasswordConstants.PasswordHash
        };
        for (var i = 0; i < totalInvokes; i++)
        {
            var grain = Cluster.GrainFactory.GetGrain<IPasswordVerifierGrain>(Guid.NewGuid());
            tasks.Add(grain.StartWorkAndPollUntilResult(request));
        }

        await Task.WhenAll(tasks);

        tasks.Select(task => task.Result).Should().OnlyContain(result => true);
    }

    /// <summary>
    /// This should be more than enough grains to overload the server.  
    /// Should be a "pretty decent" test to show that the <see cref="LimitedConcurrencyLevelTaskScheduler"/> is doing
    /// what is intended, by not allowing "more work than the CPU can handle", while still leaving enough room for
    /// Orleans messaging to not get overloaded.
    /// </summary>
    /// <returns></returns>
    [Fact, Trait(Traits.Category, Traits.Categories.LongRunning)]
    public async Task WhenGivenLargeNumberOfRequests_SystemShouldNotBecomeOverloaded()
    {
        var tasks = new List<Task<PasswordVerifierResult>>();
        var request = new PasswordVerifierRequest
        {
            Password = PasswordConstants.Password,
            PasswordHash = PasswordConstants.PasswordHash
        };
        for (var i = 0; i < 10_000; i++)
        {
            var grain = Cluster.GrainFactory.GetGrain<IPasswordVerifierGrain>(Guid.NewGuid());
            tasks.Add(grain.StartWorkAndPollUntilResult(request));
        }

        await Task.WhenAll(tasks);

        tasks.Select(task => task.Result).Should().OnlyContain(result => true);
    }

    [Fact]
    public async Task WhenGrainNotStarted_ShouldReturnStatusNotStartedOnGetStatus()
    {
        var grain = Cluster.GrainFactory.GetGrain<IGrainThatWaitsSetTimePriorToExceptionResultBecomingAvailable>(Guid.NewGuid());

        var status = await grain.GetWorkStatus();
        status.Should().Be(Enums.SyncWorkStatus.NotStarted);
    }

    [Fact]
    public async Task WhenGrainStartedButWorkNotCompleted_ShouldReturnStatusRunningOnGetStatus()
    {
        var delay = 2500;
        var grain = Cluster.GrainFactory.GetGrain<IGrainThatWaitsSetTimePriorToSuccessResultBecomingAvailable>(Guid.NewGuid());
        await grain.Start(new TestDelaySuccessRequest()
        {
            Started = DateTime.UtcNow,
            MsDelayPriorToResult = delay
        }, _cancellationTokenSource.Token);

        var status = await grain.GetWorkStatus();

        status.Should().Be(Enums.SyncWorkStatus.Running);

        await Task.Delay(delay * 2);

        status = await grain.GetWorkStatus();

        status.Should().Be(Enums.SyncWorkStatus.Completed);
    }

    [Fact]
    public async Task WhenGrainStartedButWorkNotCompleted_ShouldThrowWhenAttemptingToRetrieveResults()
    {
        var delay = 5000;
        var grain = Cluster.GrainFactory.GetGrain<IGrainThatWaitsSetTimePriorToSuccessResultBecomingAvailable>(Guid.NewGuid());
        await grain.Start(new TestDelaySuccessRequest()
        {
            Started = DateTime.UtcNow,
            MsDelayPriorToResult = delay
        }, _cancellationTokenSource.Token);

        var action = new Func<Task>(async () => await grain.GetResult());

        await action.Should().ThrowAsync<InvalidStateException>();
    }

    [Fact]
    public async Task WhenGrainExceptionStartedButWorkNotCompleted_ShouldReturnStatusRunningOnGetStatus()
    {
        var delay = 2500;
        var grain = Cluster.GrainFactory.GetGrain<IGrainThatWaitsSetTimePriorToExceptionResultBecomingAvailable>(Guid.NewGuid());
        await grain.Start(new TestDelayExceptionRequest()
        {
            MsDelayPriorToResult = delay
        }, _cancellationTokenSource.Token);

        var status = await grain.GetWorkStatus();

        status.Should().Be(Enums.SyncWorkStatus.Running);

        await Task.Delay(delay * 2);

        status = await grain.GetWorkStatus();

        status.Should().Be(Enums.SyncWorkStatus.Faulted);
    }

    [Fact]
    public async Task WhenExceptionGrainStartedButWorkNotCompleted_ShouldThrowWhenAttemptingToRetrieveResults()
    {
        var delay = 5000;
        var grain = Cluster.GrainFactory.GetGrain<IGrainThatWaitsSetTimePriorToExceptionResultBecomingAvailable>(Guid.NewGuid());
        await grain.Start(new TestDelayExceptionRequest()
        {
            MsDelayPriorToResult = delay
        }, _cancellationTokenSource.Token);

        var action = new Func<Task>(async () => await grain.GetException());

        await action.Should().ThrowAsync<InvalidStateException>();
    }

    [Fact]
    public async Task WhenSuccessGrainWorkCompleted_ShouldContainValueForGetResult()
    {
        var started = DateTime.UtcNow;

        var request = new TestDelaySuccessRequest()
        {
            Started = started,
            MsDelayPriorToResult = 10
        };

        var grain = Cluster.GrainFactory.GetGrain<IGrainThatWaitsSetTimePriorToSuccessResultBecomingAvailable>(Guid.NewGuid());
        var result = await grain.StartWorkAndPollUntilResult(request);

        result.Started.Should().Be(started);
    }

    [Fact]
    public async Task WhenSuccessGrainWorkCompleted_ShouldThrowForGetException()
    {
        var started = DateTime.UtcNow;

        var request = new TestDelaySuccessRequest()
        {
            Started = started,
            MsDelayPriorToResult = 10
        };

        var grain = Cluster.GrainFactory.GetGrain<IGrainThatWaitsSetTimePriorToSuccessResultBecomingAvailable>(Guid.NewGuid());
        await grain.Start(request);

        var status = await grain.GetWorkStatus();
        while (status == Enums.SyncWorkStatus.Running)
        {
            await Task.Delay(100);
            status = await grain.GetWorkStatus();
        }

        var action = new Func<Task>(() => grain.GetException());

        await action.Should().ThrowAsync<InvalidStateException>();
    }

    [Fact]
    public async Task WhenExceptionGrainWorkCompleted_ShouldGetExceptionForGetException()
    {
        var request = new TestDelayExceptionRequest()
        {
            MsDelayPriorToResult = 10
        };

        var grain = Cluster.GrainFactory.GetGrain<IGrainThatWaitsSetTimePriorToExceptionResultBecomingAvailable>(Guid.NewGuid());
        var action = new Func<Task>(async () => await grain.StartWorkAndPollUntilResult(request));

        await action.Should().ThrowAsync<TestGrainException>();
    }

    [Fact]
    public async Task WhenExceptionGrainWorkCompleted_ShouldThrowForGetResult()
    {
        var request = new TestDelayExceptionRequest()
        {
            MsDelayPriorToResult = 10
        };

        var grain = Cluster.GrainFactory.GetGrain<IGrainThatWaitsSetTimePriorToExceptionResultBecomingAvailable>(Guid.NewGuid());
        await grain.Start(request);

        var status = await grain.GetWorkStatus();
        while (status == Enums.SyncWorkStatus.Running)
        {
            await Task.Delay(100);
            status = await grain.GetWorkStatus();
        }

        var action = new Func<Task>(() => grain.GetResult());

        await action.Should().ThrowAsync<InvalidStateException>();
    }

    [Fact]
    public async Task WhenGrainHasCancellationSupport_ShouldRunThroughFullGrainLogicIfNoCancellation()
    {
        var request = new SampleCancellationRequest
        {
            StartingValue = 1,
            EnumerationDelay = TimeSpan.FromMilliseconds(10),
            EnumerationMax = 1_000
        };

        var grain = Cluster.GrainFactory.GetGrain<ICancellableGrain>(Guid.NewGuid());

        var result = await grain.StartWorkAndPollUntilResult(request);

        result.EndingValue.Should().BeGreaterOrEqualTo(1000);
    }

    [Fact]
    public async Task WhenGrainHasCancellationSupport_CanReturnResultAtCancellation()
    {
        var request = new SampleCancellationRequest
        {
            StartingValue = 1,
            EnumerationDelay = TimeSpan.FromMilliseconds(10),
            EnumerationMax = 1_000,
            ThrowOnCancel = false,
        };

        var grain = Cluster.GrainFactory.GetGrain<ICancellableGrain>(Guid.NewGuid());

        var cancellationToken = _cancellationTokenSource.Token;

        _ = await grain.Start(request, cancellationToken);
        await Task.Delay(TimeSpan.FromSeconds(1));
        await _cancellationTokenSource.Cancel();

        // Since sending the cancellation is not instantaneous, wait until the work status has changed from running
        var status = await grain.GetWorkStatus();
        while (status == Enums.SyncWorkStatus.Running)
        {
            await Task.Delay(100);
            status = await grain.GetWorkStatus();
        }

        var result = await grain.GetResult();

        result!.EndingValue.Should().BeGreaterThan(1);
        result!.EndingValue.Should().BeLessThan(1_000);
    }

    [Fact]
    public async Task WhenGrainHasCancellationSupport_CanThrow()
    {
        var request = new SampleCancellationRequest
        {
            StartingValue = 1,
            EnumerationDelay = TimeSpan.FromMilliseconds(10),
            EnumerationMax = 1_000,
            ThrowOnCancel = true,
        };

        var grain = Cluster.GrainFactory.GetGrain<ICancellableGrain>(Guid.NewGuid());

        var cancellationToken = _cancellationTokenSource.Token;

        _ = await grain.Start(request, cancellationToken);
        await Task.Delay(TimeSpan.FromSeconds(1));
        await _cancellationTokenSource.Cancel();

        // Since sending the cancellation is not instantaneous, wait until the work status has changed from running
        var status = await grain.GetWorkStatus();
        while (status == Enums.SyncWorkStatus.Running)
        {
            await Task.Delay(100);
            status = await grain.GetWorkStatus();
        }

        var result = await grain.GetException();

        result.Should().BeOfType<OperationCanceledException>();
    }
}
