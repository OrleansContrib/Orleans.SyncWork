using System.Threading.Tasks;

namespace Orleans.SyncWork.Demo.Services.Grains;

public interface IHelloWorld : IGrainWithGuidKey
{
    Task<string> GetGreeting(string name);
}
