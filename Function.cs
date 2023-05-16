var services = ServiceFactory.GetServiceProvider();
var innerHandler = new InnerHandler(services);
var serializer = new DefaultLambdaJsonSerializer(x => x.PropertyNameCaseInsensitive = true);

// The function handler that will be called for each Lambda event
var handler = async (APIGatewayProxyRequest request) =>
{
    var socketRequest = request.To<OnDefaultCommand>(serializer);
    return await innerHandler.Handle(socketRequest);
};

await LambdaBootstrapBuilder.Create(handler, serializer)
    .Build()
    .RunAsync();