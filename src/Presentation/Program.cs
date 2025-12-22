using Application.Configurations;
using Infrastructure.Configurations;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Presentation.Configurations;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddPresentationServices()
    .AddInfrastructureServices()
    .AddApplicationServices()
    .AddAspireClientConfiguration();

WebApplication app = builder
    .Build();

app.AddWebApplicationConfiguration();

using (IServiceScope scope = app.Services.CreateScope())
{
    TodoDbContext db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    await db.Database.MigrateAsync();
}

await app.RunAsync();
