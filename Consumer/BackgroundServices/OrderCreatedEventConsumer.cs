
using Amazon.SQS;
using Amazon.SQS.Model;

namespace Consumer.BackgroundServices
{
    public class OrderCreatedEventConsumer(IAmazonSQS amazonSQS) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string queueName = "order-created-queue";
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

            ReceiveMessageRequest receiveMessageRequest = new()
            {
                QueueUrl = queueUrl
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                ReceiveMessageResponse receiveMessageResponse = await amazonSQS.ReceiveMessageAsync(receiveMessageRequest);
                if (receiveMessageResponse.Messages.Count > 0)
                    foreach (Message message in receiveMessageResponse.Messages)
                    {
                        //OrderCreatedEvent _message = JsonSerializer.Deserialize<OrderCreatedEvent>(message.Body);
                        Console.WriteLine(message.Body);
                        await Task.Delay(1000);
                        await amazonSQS.DeleteMessageAsync(queueUrl, message.ReceiptHandle);
                    }
            }
        }
    }
}
