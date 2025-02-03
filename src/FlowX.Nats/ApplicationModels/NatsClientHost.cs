using FlowX.Nats.Statics;

namespace FlowX.Nats.ApplicationModels;

public class NatsClientHost
{
    public void Url(string url) => NatsStatics.NatsUrl = url;
    public void TopicPrefix(string topicPrefix) => NatsStatics.NatsTopicPrefix = topicPrefix;
}