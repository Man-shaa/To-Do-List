using Application;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Presentation;
using Presentation.Endpoints;
using Presentation.ExceptionHandlers;

var builder = WebApplication.CreateBuilder(args);

builder.AddPresentation()
    .AddInfrastructure()
    .AddApplication();

var app = builder.Build();

app.UseApplicationExceptionHandling();

app.MapTodoEndpoints();
app.MapOpenApi();
app.MapSwagger();
app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    db.Database.Migrate();
}

await app.RunAsync();
