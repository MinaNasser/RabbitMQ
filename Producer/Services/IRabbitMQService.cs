namespace Producer.Services
{
    public interface IRabbitMQService
    {
        Task PublishMessageAsync<T>(T message);
    }
}
