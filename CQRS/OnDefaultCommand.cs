namespace Flyingdarts.Backend.Signalling.OnDefault.CQRS
{
    public class OnDefaultCommand : IRequest<APIGatewayProxyResponse>
    {
        public string ConnectionId { get; set; }
        internal AmazonApiGatewayManagementApiClient ApiGatewayManagementApiClient { get; set; }
    }
}
