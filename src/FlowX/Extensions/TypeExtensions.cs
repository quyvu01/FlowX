namespace FlowX.Extensions;

public static class TypeExtensions
{
    extension(Type type)
    {
        /// <summary>
        /// Gets the assembly-qualified name of a type in the format "FullName,AssemblyName".
        /// </summary>
        /// <returns>A string containing the type's full name and assembly name.</returns>
        public string GetAssemblyName() => $"{type.FullName},{type.Assembly.GetName().Name}";
    }
}