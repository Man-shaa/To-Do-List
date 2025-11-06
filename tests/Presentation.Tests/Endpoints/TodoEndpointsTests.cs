using System.Net.Http.Json;
using Domain.Entities;
using Infrastructure.Repositories.DTOs;
using Presentation.Tests.fixture;
using Xunit;

namespace Presentation.Tests.Endpoints;

public sealed class TodoEndpointsTests(TestingFixture fixture) : IClassFixture<TestingFixture>
{
    [Fact]
    public async Task GetAllTodos_ReturnsSuccess()
    {
        var client = await fixture.CreateHttpClient();
        
        var response = await client.GetAsync("/todos");

        await Verify(response);
    }
}
