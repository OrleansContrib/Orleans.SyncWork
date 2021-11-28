﻿using System;
using System.Threading.Tasks;
using FluentAssertions;
using Orleans.SyncWork.Demo.Api.Services.Grains;
using Orleans.SyncWork.Tests.TestClusters;
using Xunit;

namespace Orleans.SyncWork.Tests;

/// <summary>
/// Baseline "normal" grain tests.
/// </summary>
public class HelloWorldTests : ClusterTestBase
{
    public HelloWorldTests(ClusterFixture fixture) : base(fixture) { }

    [Theory]
    [InlineData("Kritner")]
    [InlineData("Applesauce")]
    public async Task WhenGivenName_ShouldReturnNameWithinString(string name)
    {
        var grain = Cluster.GrainFactory.GetGrain<IHelloWorld>(Guid.Empty);
        var result = await grain.GetGreeting(name);

        result.Should().Contain(name);
    }
}
