using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Messaging.ServiceBus;
using Azure.Identity;


var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

var serviceBusNamespace = Environment.GetEnvironmentVariable("ServiceBusNamespace");
builder.Services.AddSingleton(
    provider => new ServiceBusClient(serviceBusNamespace, new DefaultAzureCredential()));

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
