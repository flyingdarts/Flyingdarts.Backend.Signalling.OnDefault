using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Flyingdarts.Backend.Signalling.OnDefault.CQRS;
using Flyingdarts.Lambdas.Shared;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

public class InnerHandler
{
    private readonly IMediator _mediator;

    public InnerHandler()
    {
    }
    public InnerHandler(ServiceProvider serviceProvider)
    {
        _mediator = serviceProvider.GetRequiredService<IMediator>();
    }
    public async Task<APIGatewayProxyResponse> Handle(SocketMessage<OnDefaultCommand> request)
    {
        if (request?.Message is null)
            throw new BadRequestException("Unable to parse request.", typeof(OnDefaultCommand));
        request.Message.ConnectionId = request.ConnectionId;

        return await _mediator.Send(request.Message);
    }
}