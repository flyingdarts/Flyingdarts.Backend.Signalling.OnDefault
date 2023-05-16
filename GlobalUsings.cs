﻿global using Amazon.ApiGatewayManagementApi;
global using Amazon.ApiGatewayManagementApi.Model;
global using Amazon.DynamoDBv2;
global using Amazon.DynamoDBv2.Model;
global using Amazon.Lambda.APIGatewayEvents;
global using Amazon.Lambda.RuntimeSupport;
global using Amazon.Lambda.Serialization.SystemTextJson;
global using FluentValidation;
global using Flyingdarts.Backend.Signalling.OnDefault.CQRS;
global using Flyingdarts.Lambdas.Shared;
global using MediatR;
global using Microsoft.Extensions.DependencyInjection;
global using System;
global using System.IO;
global using System.Text;
global using System.Text.Json;
global using System.Threading;
global using System.Threading.Tasks;