using Shared.Common;

namespace Shared.Events
{
    public class OrderCreatedEvent : IEvent
    {
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime CreatedDate { get; } = DateTime.UtcNow;
    }
}
