using System.Text.Json.Serialization;
using AutoCtor;
using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateSlimBuilder(args);

// Replace the service provider with our own
builder.Host.UseServiceProviderFactory(new ServiceProviderFactory());

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

var todosApi = app.MapGroup("/todos");
todosApi.MapGet("/", (TodoService todoProvider) => todoProvider.GetTodos())
        .WithName("GetTodos");

todosApi.MapGet("/{id}", Results<Ok<Todo>, NotFound> (int id, TodoService todoProvider) =>
    todoProvider.GetTodos().FirstOrDefault(a => a.Id == id) is { } todo
        ? TypedResults.Ok(todo)
        : TypedResults.NotFound())
    .WithName("GetTodoById");

app.Run();

// ----------------------------------------------------------------------------

internal record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

[JsonSerializable(typeof(Todo[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext;

// ----------------------------------------------------------------------------

[ServiceProvider(Fallback = nameof(_fallback))]
[Singleton<TodoService>]
internal partial class ExampleServiceProvider(IServiceProvider fallback)
{
    private readonly IServiceProvider _fallback = fallback;
}

internal class ServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
{
    public IServiceCollection CreateBuilder(IServiceCollection services) => services;

    public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
    {
        ExampleServiceProvider provider = null!;
        containerBuilder.AddSingleton<IHttpContextFactory>(_ => new DefaultHttpContextFactory(provider));
        return provider = new ExampleServiceProvider(containerBuilder.BuildServiceProvider(true));
    }
}

[AutoConstruct]
internal partial class TodoService
{
    public Todo[] GetTodos()
    {
        return new Todo[] {
            new(1, "Walk the dog"),
            new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
            new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
            new(4, "Clean the bathroom"),
            new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
        };
    }
}
