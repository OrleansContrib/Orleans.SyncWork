using System.Threading.Tasks;

namespace Orleans.SyncWork.Demo.Api.Services.Grains;

public interface IHelloWorld : IGrainWithGuidKey
{
    Task<string> GetGreeting(string name);
}
