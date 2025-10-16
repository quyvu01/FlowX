using FlowX.Nats.Statics;

namespace FlowX.Nats.Extensions;

internal static class Extensions
{
    public static string GetNatsSubject(this Type type) =>
        string.IsNullOrEmpty(NatsStatics.NatsTopicPrefix)
            ? $"FlowX-{type.Namespace}-{type.Name}".ToLower()
            : $"{NatsStatics.NatsTopicPrefix}-FlowX-{type.Namespace}-{type.Name}".ToLower();
}