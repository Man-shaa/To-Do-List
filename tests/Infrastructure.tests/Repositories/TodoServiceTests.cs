using Infrastructure.Repositories;
using Infrastructure.Repositories.Configurations;
using Infrastructure.Repositories.DTOs;
using Microsoft.Extensions.Options;

namespace Infrastructure.Tests.Repositories;

[Collection("TodoService.All")]
public sealed class TodoServiceTests
{
    private static TodoService CreateService(string baseUrl = "https://localhost/") =>
        new(Options.Create(new SettingsOptions { BaseUrl = new Uri(baseUrl) }));

    [Fact]
    public async Task GetAllAsync_should_return_empty_list_initially_and_same_reference_across_calls()
    {
        var todoService = CreateService();

        var list1 = await todoService.GetAllAsync(CancellationToken.None);
        Assert.Empty(list1);

        var list2 = await todoService.GetAllAsync(CancellationToken.None);
        Assert.Same(list1, list2);
    }

    [Fact]
    public async Task CreateAsync_should_use_title_url_and_default_order_equal_to_id_when_order_missing()
    {
        var todoService = CreateService("https://example/");

        var dto = new TodoCreateDto { Title = "A" };
        var todo = await todoService.CreateAsync(dto, CancellationToken.None);

        Assert.Equal("A", todo.Title);
        Assert.Equal(new Uri($"https://example/todos/{todo.Id}"), todo.Url);
        Assert.Equal(todo.Id, todo.Order);
    }

    [Fact]
    public async Task CreateAsync_should_respect_explicit_order_when_provided()
    {
        var todoService = CreateService();

        var dto = new TodoCreateDto { Title = "B", Order = 7 };
        var todo = await todoService.CreateAsync(dto, CancellationToken.None);

        Assert.Equal("B", todo.Title);
        Assert.Equal(7, todo.Order);
    }

    [Fact]
    public async Task CreateAsync_should_increment_identifier_and_add_to_list()
    {
        var todoService = CreateService();

        var t1 = await todoService.CreateAsync(new TodoCreateDto { Title = "T1" }, CancellationToken.None);
        var t2 = await todoService.CreateAsync(new TodoCreateDto { Title = "T2" }, CancellationToken.None);

        Assert.True(t2.Id == t1.Id + 1, "Identifiers should increment by 1");
        Assert.Equal(new Uri($"https://localhost/todos/{t2.Id}"), t2.Url);

        var all = await todoService.GetAllAsync(CancellationToken.None);
        Assert.Equal(2, all.Count);
        Assert.Contains(t1, all);
        Assert.Contains(t2, all);
    }

    [Fact]
    public async Task GetByIdAsync_should_return_existing_item_and_null_when_missing()
    {
        var todoService = CreateService();

        var t1 = await todoService.CreateAsync(new TodoCreateDto { Title = "One" }, CancellationToken.None);
        var t2 = await todoService.CreateAsync(new TodoCreateDto { Title = "Two" }, CancellationToken.None);

        var found = await todoService.GetByIdAsync(t1.Id, CancellationToken.None);
        Assert.Same(t1, found);

        var missing = await todoService.GetByIdAsync(t2.Id + 9702399, CancellationToken.None);
        Assert.Null(missing);
    }

    [Fact]
    public async Task DeleteByIdAsync_should_remove_existing_item_and_return_true()
    {
        var todoService = CreateService();

        var t1 = await todoService.CreateAsync(new TodoCreateDto { Title = "One" }, CancellationToken.None);
        var t2 = await todoService.CreateAsync(new TodoCreateDto { Title = "Two" }, CancellationToken.None);

        var result = await todoService.DeleteByIdAsync(t1, CancellationToken.None);
        Assert.True(result);

        var all = await todoService.GetAllAsync(CancellationToken.None);
        Assert.Single(all);
        Assert.DoesNotContain(t1, all);
        Assert.Contains(t2, all);

        var byId = await todoService.GetByIdAsync(t1.Id, CancellationToken.None);
        Assert.Null(byId);
    }

    [Fact]
    public async Task
        DeleteByIdAsync_should_return_true_for_nonexistent_item_when_argument_is_not_null_and_leave_list_unchanged()
    {
        var todoService = CreateService();

        var t1 = await todoService.CreateAsync(new TodoCreateDto { Title = "One" }, CancellationToken.None);
        _ = await todoService.DeleteByIdAsync(t1, CancellationToken.None);

        var countBefore = (await todoService.GetAllAsync(CancellationToken.None)).Count;

        var resultAgain = await todoService.DeleteByIdAsync(t1, CancellationToken.None);
        Assert.True(resultAgain);

        var countAfter = (await todoService.GetAllAsync(CancellationToken.None)).Count;
        Assert.Equal(countBefore, countAfter);
    }

    [Fact]
    public async Task DeleteByIdAsync_should_return_false_when_argument_is_null()
    {
        var todoService = CreateService();

        var result = await todoService.DeleteByIdAsync(null, CancellationToken.None);
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAllAsync_should_clear_all_items_and_be_idempotent()
    {
        var todoService = CreateService();

        await todoService.CreateAsync(new TodoCreateDto { Title = "A" }, CancellationToken.None);
        await todoService.CreateAsync(new TodoCreateDto { Title = "B" }, CancellationToken.None);

        await todoService.DeleteAllAsync(CancellationToken.None);

        var afterFirst = await todoService.GetAllAsync(CancellationToken.None);
        Assert.Empty(afterFirst);

        await todoService.DeleteAllAsync(CancellationToken.None);
        var afterSecond = await todoService.GetAllAsync(CancellationToken.None);
        Assert.Empty(afterSecond);
    }

    [Fact]
    public async Task DeleteAllAsync_should_not_reset_identity_counter()
    {
        var todoService = CreateService();

        await todoService.CreateAsync(new TodoCreateDto { Title = "A" }, CancellationToken.None);
        var t2 = await todoService.CreateAsync(new TodoCreateDto { Title = "B" }, CancellationToken.None);
        var lastIdBefore = t2.Id;

        await todoService.DeleteAllAsync(CancellationToken.None);

        var t3 = await todoService.CreateAsync(new TodoCreateDto { Title = "C" }, CancellationToken.None);
        Assert.Equal(lastIdBefore + 1, t3.Id);
    }
}
