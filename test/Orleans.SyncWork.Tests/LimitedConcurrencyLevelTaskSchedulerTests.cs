using System;
using FluentAssertions;
using Xunit;

namespace Orleans.SyncWork.Tests;

public class LimitedConcurrencyLevelTaskSchedulerTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    public void WhenProvidedConcurrencyValueAtConstruction_ShouldContainThatLevelOfMaxConcurrency(int maxDegreeOfParallelism)
    {
        var subject = new LimitedConcurrencyLevelTaskScheduler(maxDegreeOfParallelism);

        subject.MaximumConcurrencyLevel.Should().Be(maxDegreeOfParallelism);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-42)]
    public void WhenProvidedConcurrencyAtOrBelowZero_ShouldThrow(int maxDegreeOfParallelism)
    {
        Action action = () =>
        {
            _ = new LimitedConcurrencyLevelTaskScheduler(maxDegreeOfParallelism);
        };

        action.Should().Throw<ArgumentOutOfRangeException>();
    }
}
