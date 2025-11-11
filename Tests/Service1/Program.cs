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
    options.AddHandlersFromNamespaceContaining<IAssemblyMarker>();
    options.AddNats(config => config.Url("nats://localhost:4222"));
})
.AddEfCore(c => c.AddDbContexts(typeof(Service1DbContext)));

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