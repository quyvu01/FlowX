using System.Reflection;
using FlowX.EntityFrameworkCore.Extensions;
using FlowX.Extensions;
using FlowX.Nats.Extensions;
using Service1;
using Service1.Contexts;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContextPool<Service1DbContext>(options => options.UseNpgsql(
    builder.Configuration["PostgresDbSetting:ConnectionString"],
    b =>
    {
        b.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
        b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    }), 128);

builder.Services.AddFlowX(options =>
{
    options.AddModelsFromNamespaceContaining<IAssemblyMarker>();
    options.AddHandlersFromNamespaceContaining<IAssemblyMarker>();
    options.SetRetryPolicy(3, _ => TimeSpan.FromSeconds(2), (ex, _, _) =>
    {
        Console.WriteLine(ex.Message);
        return Task.CompletedTask;
    });
    options.AddDbContextDynamic<Service1DbContext>(opts =>
    {
        opts.AddDynamicRepositories();
        opts.AddDynamicUnitOfWork();
    });
    options.AddNats(config => config.Url("nats://localhost:4222"));
});

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.MapControllers();

app.UseHttpsRedirection();

app.Run();