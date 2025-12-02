using Application.Configurations;
using Infrastructure.Configurations;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Presentation.Configurations;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddPresentation()
    .AddInfrastructure()
    .AddApplication()
    .AddServiceDefaults();

var app = builder.Build();

await app.AddWebApplicationConfiguration();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    await db.Database.MigrateAsync();
}

await app.RunAsync();
