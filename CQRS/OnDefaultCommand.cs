using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Runtime;
using Amazon.Runtime.Internal;
using FluentValidation;
using Flyingdarts.Lambdas.Shared;
using MediatR;

namespace Flyingdarts.Backend.Signalling.OnDefault.CQRS
{
    public class OnDefaultCommand : IRequest<APIGatewayProxyResponse>
    {
        public string ConnectionId { get; set; }

    }
}
