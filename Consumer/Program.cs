using Amazon.SQS;
using Consumer.BackgroundServices;
using MassTransit;
using Consumers = Consumer.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonSQS>();
//builder.Services.AddHostedService<OrderCreatedEventConsumer>();

#region MassTransit
builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<Consumers.OrderCreatedEventConsumer>().Endpoint(e => e.InstanceId = "queue");

    configurator.UsingAmazonSqs((context, _configurator) =>
    {
        _configurator.Host(Amazon.RegionEndpoint.APSouth1.OriginalSystemName, hostConfigurator =>
        {
            hostConfigurator.AccessKey(builder.Configuration["AWS:AccessKey"]);
            hostConfigurator.SecretKey(builder.Configuration["AWS:SecretKey"]);
        });

        _configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter("masstransit", false));
    });
});
#endregion

var app = builder.Build();

app.Run();
