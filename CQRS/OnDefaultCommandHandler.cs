public class OnDefaultCommandHandler : IRequestHandler<OnDefaultCommand, APIGatewayProxyResponse>
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;
    public OnDefaultCommandHandler(IAmazonDynamoDB dynamoDb)
    {
        _dynamoDb = dynamoDb;
        _tableName = System.Environment.GetEnvironmentVariable("TableName");
    }
    public async Task<APIGatewayProxyResponse> Handle(OnDefaultCommand request, CancellationToken cancellationToken)
    {
        var apiGatewayManagementApiClientFactory = (Func<string, AmazonApiGatewayManagementApiClient>)((endpoint) =>
        {
            return new AmazonApiGatewayManagementApiClient(new AmazonApiGatewayManagementApiConfig
            {
                ServiceURL = endpoint
            });
        });
        // List all of the current connections. In a more advanced use case the table could be used to grab a group of connection ids for a chat group.
        var scanRequest = new ScanRequest
        {
            TableName = _tableName,
            ProjectionExpression = "ConnectionId"
        };

        var scanResponse = await _dynamoDb.ScanAsync(scanRequest);

        // Construct the IAmazonApiGatewayManagementApi which will be used to send the message to.
        var apiClient = apiGatewayManagementApiClientFactory(System.Environment.GetEnvironmentVariable("WebSocketApiUrl"));

        var socketMessage = new SocketMessage<OnDefaultCommand>
        {
            Message = request,
            Action = "default$"
        };

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(socketMessage)));

        // Loop through all of the connections and broadcast the message out to the connections.
        var count = 0;
        foreach (var item in scanResponse.Items)
        {
            var postConnectionRequest = new PostToConnectionRequest
            {
                ConnectionId = item["ConnectionId"].S,
                Data = stream
            };
                
            //context.Logger.LogInformation($"Post to connection {count}: {postConnectionRequest.ConnectionId}");
            stream.Position = 0;
            await apiClient.PostToConnectionAsync(postConnectionRequest);
            count++;
        }

        return new APIGatewayProxyResponse { StatusCode = 200, Body = JsonSerializer.Serialize(socketMessage) };
    }
}