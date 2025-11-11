using FlowX.EntityFrameworkCore.Abstractions;
using FlowX.EntityFrameworkCore.Exceptions;
using FlowX.EntityFrameworkCore.Implementations;
using FlowX.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.EntityFrameworkCore.Registries;

public class FlowXEfCoreRegister(IServiceCollection serviceCollection)
{
    private static readonly Dictionary<Type, string> DbContextMapFunction = [];
    public void AddDbContexts(Type dbContextType, params Type[] otherDbContextTypes)
    {
        List<Type> dbContextTypes = [dbContextType, ..otherDbContextTypes ?? []];
        if (dbContextTypes.Count == 0)
            throw new FlowXEntityFrameworkException.DbContextsMustNotBeEmpty();

        dbContextTypes.Distinct().ForEach(type =>
        {
            ArgumentNullException.ThrowIfNull(type);
            if (!DbContextMapFunction.TryAdd(type, nameof(AddDbContexts)))
                throw new FlowXEntityFrameworkException.DbContextTypeHasBeenRegisterBefore(type);
            serviceCollection.AddScoped<IDbContext>(sp => sp.GetService(type) is DbContext context
                ? new DbContextInternal(context)
                : throw new FlowXEntityFrameworkException.EntityFrameworkDbContextNotRegister());
        });
    }
}