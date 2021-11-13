using BenchmarkDotNet.Attributes;
using Orleans.SyncWork.Demo.Api.Services;

namespace Orleans.SyncWork.Demo.Api.Benchmark;

public class Benchy
{
    private readonly IPasswordHasher _passwordHasher = new PasswordHasher();
    const string _password = "my super neat password that's totally secure because it's super long and i don't think anyone would be able to guess it because it's so long, you know what i mean my dude?";
    const int totalNumberPerBenchmark = 20;

    [Benchmark]
    public void Serial()
    {
        for (var i = 0; i < totalNumberPerBenchmark; i++)
        {
            _passwordHasher.HashPassword(_password);
        }
    }

    [Benchmark]
    public async Task MultipleTasks()
    {
        var tasks = new List<Task>();
        for (var i = 0; i < totalNumberPerBenchmark; i++)
        {
            tasks.Add(_passwordHasher.HashPassword(_password));
        }

        await Task.WhenAll(tasks);
    }
}
