using FlowX.Abstractions;
using Service1.Contracts.Requests;

namespace Service1.Handlers;

public class CreateTestHandler : IRequestHandler<CreateTestCommand, string>
{
    public Task<string> HandleAsync(RequestContext<CreateTestCommand> requestContext)
    {
        throw new NotImplementedException();
    }
}