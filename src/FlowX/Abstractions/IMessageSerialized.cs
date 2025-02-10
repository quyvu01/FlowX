using FlowX.Messages;

namespace FlowX.Abstractions;

public interface IMessageSerialized
{
    MessageSerialized Serialize();
}