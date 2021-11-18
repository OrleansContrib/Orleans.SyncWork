using BenchmarkDotNet.Attributes;
using Orleans.SyncWork.Demo.Api.Services;

namespace Orleans.SyncWork.Demo.Api.Benchmark;

public class Benchy
{
    const int TotalNumberPerBenchmark = 50;
    private readonly IPasswordVerifier _passwordVerifier = new PasswordVerifier();

    public Benchy()
    {
        Console.WriteLine("Doots");
    }

    [Benchmark]
    public void Serial()
    {
        for (var i = 0; i < TotalNumberPerBenchmark; i++)
        {
            _passwordVerifier.VerifyPassword(IPasswordVerifier.PasswordHash, IPasswordVerifier.Password);
        }
    }

    [Benchmark]
    public async Task MultipleTasks()
    {
        var tasks = new List<Task>();
        for (var i = 0; i < TotalNumberPerBenchmark; i++)
        {
            tasks.Add(_passwordVerifier.VerifyPassword(IPasswordVerifier.PasswordHash, IPasswordVerifier.Password));
        }

        await Task.WhenAll(tasks);
    }
}
