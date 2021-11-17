using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.SyncWork;
using Orleans.SyncWork.Demo.Api;
using Orleans.SyncWork.Demo.Api.Services.Grains;

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
        var passwordVerifyGrain = grainFactory.GetGrain<ISyncWorker<PasswordVerifierRequest, PasswordVerifierResponse>>(Guid.NewGuid());
        return await passwordVerifyGrain.StartWorkAndPollUntilResult(request);
    })
    .WithName("GetPasswordVerify");

app.Run();
