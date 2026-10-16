using Shared.Messages;

namespace Producer.Services
{
    public interface IRabbitMQService
    {
        Task PublishMessageAsync(FirstDemoMessage message, string routingKey);
    }
}
