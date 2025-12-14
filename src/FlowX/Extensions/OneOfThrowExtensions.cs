using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Extensions;

public static class OneOfThrowExtensions
{
    public static void ThrowIfError<T>(this OneOf<T, Error> oneOf)
    {
        if (oneOf.IsT1) throw oneOf.AsT1;
    }
}