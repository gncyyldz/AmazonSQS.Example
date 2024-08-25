using MassTransit;
using Shared.Events;

namespace Consumer.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    {
        public Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            Console.WriteLine($"{context.Message.OrderId} - {context.Message.CustomerId}");
            return Task.CompletedTask;
        }
    }
}
