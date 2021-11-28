using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.SyncWork;
using Orleans.SyncWork.Demo.Api;
using Orleans.SyncWork.Demo.Services.Grains;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var orleans = await OrleansConfigurationHelper.StartSilo();
builder.Services.AddSingleton(orleans);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

var grainFactory = orleans.Services.GetRequiredService<IGrainFactory>();

app
    .MapGet("/helloWorld", async (string name) =>
    {
        var helloWorldGrain = grainFactory.GetGrain<IHelloWorld>(Guid.Empty);
        return await helloWorldGrain.GetGreeting(name);
    })
    .WithName("GetHelloWorldGreeting");

app
    .MapPost("/passwordVerifier", async (PasswordVerifierRequest request) =>
    {
        var passwordVerifyGrain = grainFactory.GetGrain<ISyncWorker<PasswordVerifierRequest, PasswordVerifierResult>>(Guid.NewGuid());
        return await passwordVerifyGrain.StartWorkAndPollUntilResult(request);
    })
    .WithName("GetPasswordVerify");

app
    .MapPost("/passwordVerifierBunchoRequests", async (PasswordVerifierRequest request) =>
    {
        var tasks = new List<Task<PasswordVerifierResult>>();

        for (var i = 0; i < 10_000; i++)
        {
            var passwordVerifyGrain = grainFactory.GetGrain<ISyncWorker<PasswordVerifierRequest, PasswordVerifierResult>>(Guid.NewGuid());
            tasks.Add(passwordVerifyGrain.StartWorkAndPollUntilResult(request));
        }
        
        await Task.WhenAll(tasks);

        var allGood = tasks
        .Select(task => task.Result)
        .All(result => result.IsValid);

        return new PasswordVerifierResult()
        {
            IsValid = allGood
        };
    })
    .WithName("GetPasswordVerifyBunchoRequests");

app.Run();
