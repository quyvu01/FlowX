namespace FlowX.EntityFrameworkCore.Exceptions;

public static class FlowXEntityFrameworkException
{
    public class DbContextsMustNotBeEmpty()
        : Exception("There are no any db contexts on AddDbContexts() method");

    public class DbContextTypeHasBeenRegisterBefore(Type dbContextType) : Exception(
        $"DbContext type {dbContextType.Name} already registered!");

    public class EntityFrameworkDbContextNotRegister() : Exception("DbContext must be registered first!");

    public class ModelDoesNotBelongToDbContext(Type modelType)
        : Exception($"Model : {modelType.Name} does not belong to db context!");
}