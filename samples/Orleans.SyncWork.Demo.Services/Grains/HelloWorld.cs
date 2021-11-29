using System.Threading.Tasks;

namespace Orleans.SyncWork.Demo.Services.Grains
{
    public class HelloWorld : Grain, IHelloWorld
    {
        public Task<string> GetGreeting(string name)
        {
            return Task.FromResult($"Hello {name}!");
        }
    }
}
