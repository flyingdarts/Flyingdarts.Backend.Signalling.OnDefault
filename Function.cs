using Amazon.DynamoDBv2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Runtime;

var dynamoDbClient = new AmazonDynamoDBClient();
var tableName = Environment.GetEnvironmentVariable("TableName")!;
var webSocketApiUrl = Environment.GetEnvironmentVariable("WebSocketApiUrl")!;
var apiGatewayManagementApiClientFactory = (Func<string, AmazonApiGatewayManagementApiClient>)((endpoint) =>
{
    return new AmazonApiGatewayManagementApiClient(new AmazonApiGatewayManagementApiConfig
    {
        ServiceURL = endpoint
    });
});

// The function handler that will be called for each Lambda event
var handler = async (APIGatewayProxyRequest request, ILambdaContext context) =>
{
    try
    {
        // The body will look something like this: {"message":"sendmessage", "data":"What are you doing?"}
        var body = JsonDocument.Parse(request.Body);

        // Grab the message from the JSON body which is the message to broadcasted.
        JsonElement messageProperty;
        if (!body.RootElement.TryGetProperty("message", out messageProperty))
        {
            context.Logger.LogInformation("Failed to find data element in JSON document");
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }

        // Grab the owner from the JSON body which is the message to broadcasted.
        JsonElement ownerProperty;
        if (!body.RootElement.TryGetProperty("owner", out ownerProperty))
        {
            context.Logger.LogInformation("Failed to find data element in JSON document");
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }

        // Grab the date from the JSON body which is the message to broadcasted.
        JsonElement dateProperty;
        if (!body.RootElement.TryGetProperty("owner", out dateProperty))
        {
            context.Logger.LogInformation("Failed to find data element in JSON document");
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }

        var data = new { message = messageProperty.ToString(), owner = ownerProperty.ToString(), date = dateProperty.ToString() };
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data)));

        // List all of the current connections. In a more advanced use case the table could be used to grab a group of connection ids for a chat group.
        var scanRequest = new ScanRequest
        {
            TableName = tableName,
            ProjectionExpression = "ConnectionId"
        };

        var scanResponse = await dynamoDbClient.ScanAsync(scanRequest);

        // Construct the IAmazonApiGatewayManagementApi which will be used to send the message to.
        var apiClient = apiGatewayManagementApiClientFactory(webSocketApiUrl);

        // Loop through all of the connections and broadcast the message out to the connections.
        var count = 0;
        foreach (var item in scanResponse.Items)
        {
            var postConnectionRequest = new PostToConnectionRequest
            {
                ConnectionId = item["ConnectionId"].S,
                Data = stream
            };

            try
            {
                context.Logger.LogInformation($"Post to connection {count}: {postConnectionRequest.ConnectionId}");
                stream.Position = 0;
                await apiClient.PostToConnectionAsync(postConnectionRequest);
                count++;
            }
            catch (AmazonServiceException e)
            {
                // API Gateway returns a status of 410 GONE then the connection is no
                // longer available. If this happens, delete the identifier
                // from our DynamoDB table.
                if (e.StatusCode == HttpStatusCode.Gone)
                {
                    var ddbDeleteRequest = new DeleteItemRequest
                    {
                        TableName = tableName,
                        Key = new Dictionary<string, AttributeValue>
                        {
                            { "ConnectionId", new AttributeValue { S = postConnectionRequest.ConnectionId } }
                        }
                    };

                    context.Logger.LogInformation($"Deleting gone connection: {postConnectionRequest.ConnectionId}");
                    await dynamoDbClient.DeleteItemAsync(ddbDeleteRequest);
                }
                else
                {
                    context.Logger.LogInformation(
                        $"Error posting message to {postConnectionRequest.ConnectionId}: {e.Message}");
                    context.Logger.LogInformation(e.StackTrace);
                }
            }
        }

        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = "Data sent to " + count + " connection" + (count == 1 ? "" : "s")
        };
    }
    catch (Exception e)
    {
        context.Logger.LogInformation("Error disconnecting: " + e.Message);
        context.Logger.LogInformation(e.StackTrace);
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.InternalServerError,
            Body = $"Failed to send message: {e.Message}"
        };
    }
};

await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
    .Build()
    .RunAsync();


public class WebSocketMessage
{
    public string Date { get; set; }
    public string Message { get; set; }
    public string Owner { get; set; }
    public bool Received { get; set; }
}