﻿using System.Threading.Tasks;

namespace Orleans.SyncWork.Demo.Api.Services.Grains
{
    public class HelloWorld : Grain, IHelloWorld
    {
        public Task<string> GetGreeting(string name)
        {
            return Task.FromResult($"Hello {name}!");
        }
    }
}
