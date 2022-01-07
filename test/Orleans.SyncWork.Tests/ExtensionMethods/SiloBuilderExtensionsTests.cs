using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Orleans.Hosting;
using Orleans.SyncWork.ExtensionMethods;
using Xunit;

namespace Orleans.SyncWork.Tests.ExtensionMethods;

public class SiloBuilderExtensionsTests
{
    [Fact]
    public void WhenNotCallingConfigure_ShouldNotResolveLimitedConcurrencyScheduler()
    {
        var builder = new HostBuilder()
            .UseOrleans(siloBuilder => siloBuilder.UseLocalhostClustering());

        var host = builder.Build();
        var scheduler = (LimitedConcurrencyLevelTaskScheduler)host.Services.GetService(typeof(LimitedConcurrencyLevelTaskScheduler));

        scheduler.Should().BeNull("The extension method was not used to register the scheduler");
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    public void WhenCallingConfigure_ShouldRegisterLimitedConcurrencyScheduler(int maxSyncWorkConcurrency)
    {
        var builder = new HostBuilder()
            .UseOrleans(builder =>
            {
                builder.ConfigureSyncWorkAbstraction(maxSyncWorkConcurrency);        
                builder.UseLocalhostClustering();
            });

        var host = builder.Build();
        var scheduler = (LimitedConcurrencyLevelTaskScheduler)host.Services.GetService(typeof(LimitedConcurrencyLevelTaskScheduler));

        scheduler.Should().NotBeNull("the extension method was to registered the scheduler");
        scheduler?.MaximumConcurrencyLevel.Should().Be(maxSyncWorkConcurrency,
            "the scheduler should have the registered level of maximum concurrency");
    }
}
