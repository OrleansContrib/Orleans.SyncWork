using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Orleans.Hosting;
using Xunit;

namespace Orleans.SyncWork.Tests.ExtensionMethods;

public class SiloHostBuilderExtensionsTests
{
    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public void WhenCallingConfigure_ShouldRegisterLimitedConcurrencyScheduler(int maxSyncWorkConcurrency)
    {
        var builder = new SiloHostBuilder();
        Orleans.SyncWork.ExtensionMethods.SiloHostBuilderExtensions.ConfigureSyncWorkAbstraction(builder, maxSyncWorkConcurrency);

        builder.UseLocalhostClustering();
            
        var host = builder.Build();
        var scheduler = (LimitedConcurrencyLevelTaskScheduler)host.Services.GetService(typeof(LimitedConcurrencyLevelTaskScheduler));

        scheduler.Should().NotBeNull("the extension method was to registered the scheduler");
        scheduler?.MaximumConcurrencyLevel.Should().Be(maxSyncWorkConcurrency,
            "the scheduler should have the registered level of maximum concurrency");
    }
}
