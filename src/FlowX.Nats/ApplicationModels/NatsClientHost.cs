using FlowX.Nats.Statics;
using NATS.Client.Core;

namespace FlowX.Nats.ApplicationModels;

public class NatsClientHost
{
    public NatsOpts NatsOptions { get; private set; }
    public string NatsUrl { get; private set; }
    public void Url(string url) => NatsUrl = url;
    public void TopicPrefix(string topicPrefix) => NatsStatics.NatsTopicPrefix = topicPrefix;
    public void NatsOpts(NatsOpts options = null) => NatsOptions = options;
}