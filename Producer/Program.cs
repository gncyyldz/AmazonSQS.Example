using Amazon.SQS;
using Amazon.SQS.Model;
using MassTransit;
using Shared.Events;
using System.Text.Json;
using static MassTransit.Monitoring.Performance.BuiltInCounters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonSQS>();

#region MassTransit
builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingAmazonSqs((context, _configurator) =>
    {
        _configurator.Host(Amazon.RegionEndpoint.APSouth1.OriginalSystemName, hostConfigurator =>
        {
            hostConfigurator.AccessKey(builder.Configuration["AWS:AccessKey"]);
            hostConfigurator.SecretKey(builder.Configuration["AWS:SecretKey"]);
        });
    });
});
#endregion

var app = builder.Build();

app.MapGet("/create-order", async (IAmazonSQS amazonSQS) =>
{
    string queueName = "order-created-queue";

    OrderCreatedEvent orderCreatedEvent = new()
    {
        CustomerId = Guid.NewGuid(),
        OrderId = Guid.NewGuid()
    };

    string queueUrl = string.Empty;

    try
    {
        GetQueueUrlResponse getQueueUrlResponse = await amazonSQS.GetQueueUrlAsync(queueName);
        queueUrl = getQueueUrlResponse.QueueUrl;
    }
    catch (QueueDoesNotExistException)
    {
        CreateQueueResponse createQueueResponse = await amazonSQS.CreateQueueAsync(queueName);
        queueUrl = createQueueResponse.QueueUrl;
    }

    SendMessageRequest sendMessageRequest = new()
    {
        QueueUrl = queueUrl,
        MessageBody = JsonSerializer.Serialize(orderCreatedEvent)
    };

    await amazonSQS.SendMessageAsync(sendMessageRequest);
    return Results.Ok();
});

#region MassTransit
app.MapGet("/create-order-masstransit", async (IPublishEndpoint publishEndpoint, ISendEndpointProvider sendEndpointProvider) =>
{
    OrderCreatedEvent orderCreatedEvent = new()
    {
        CustomerId = Guid.NewGuid(),
        OrderId = Guid.NewGuid()
    };

    #region Publish
    //await publishEndpoint.Publish(orderCreatedEvent);
    #endregion

    #region Send
    var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:masstransit-order-created-event-queue"));
    await sendEndpoint.Send(orderCreatedEvent);
    #endregion

    return Results.Ok();
});
#endregion

app.Run();
