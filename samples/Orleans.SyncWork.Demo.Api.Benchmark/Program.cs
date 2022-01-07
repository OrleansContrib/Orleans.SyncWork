using BenchmarkDotNet.Running;
using Orleans.SyncWork.Demo.Api.Benchmark;

BenchmarkingSiloHost.GetTestCluster();

BenchmarkRunner.Run<Benchy>();
