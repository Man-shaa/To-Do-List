using System;
using Microsoft.Extensions.Options;
using Todo.Infrastructure.Repositories;
using Todo.Presentation.Configurations;
using ToDo.Presentation.Configurations;
using Xunit;

namespace Application.Tests.Infrastructure.Repositories;



public class TodoServiceTests
{
    [Fact]
    public void UselessMethod_ShouldReturnHello_WhenInputIsHello()
    {
        var settings = new SettingsOptions
        {
            BaseUrl = new Uri("https://localhost:7214"),
        };
        var options = Options.Create(settings);
        var service = new TodoService(options);

        const string stringSent = "Hello";
        var result = service.UselessMethod(stringSent);
        
        Assert.Equal(stringSent, result);
    }
}
