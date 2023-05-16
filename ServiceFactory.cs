using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Flyingdarts.Backend.Signalling.OnDefault.CQRS;

public static class ServiceFactory
{
    public static ServiceProvider GetServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddAWSService<IAmazonDynamoDB>();
        services.AddValidatorsFromAssemblyContaining<OnDefaultCommandValidator>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(OnDefaultCommand).Assembly));
        return services.BuildServiceProvider();
    }
}