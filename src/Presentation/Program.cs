using Application;
using Infrastructure;
using Presentation;
using Presentation.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure();
builder.Services.AddApplication();
builder.Services.AddPresentation();

var app = builder.Build();

app.MapTodoEndpoints();

await app.RunAsync();
