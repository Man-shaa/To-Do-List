using AppHost.Configurations;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

builder.AddPresentationProject();

await builder
    .Build()
    .RunAsync();
