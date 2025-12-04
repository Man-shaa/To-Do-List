using AppHost.Configurations;
using AspireConfiguration;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres();
var kafka = builder.AddKafka();

builder.AddTodoProject(postgres, kafka, AspireResourcesName.Dapr.TodoConsumers);

await builder
    .Build()
    .RunAsync();
