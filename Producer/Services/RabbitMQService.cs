
using RabbitMQ.Client;
using Shared.Configurations;
using Shared.Messages;
using System.Text.Json;

namespace Producer.Services
{
    public class RabbitMQService : IRabbitMQService
    {
        private readonly RabbitMQConfiguration _rabbitMQConfiguration;
        private readonly ConnectionFactory _connectionFactory;

        public RabbitMQService(
            RabbitMQConfiguration rabbitMQConfiguration)
        {
            _rabbitMQConfiguration = rabbitMQConfiguration;

            _connectionFactory = new ConnectionFactory
            {
                HostName = _rabbitMQConfiguration.Server,
                Port = _rabbitMQConfiguration.Port,
                UserName = _rabbitMQConfiguration.UserName,
                Password = _rabbitMQConfiguration.Password,
                VirtualHost = _rabbitMQConfiguration.VirtualHost
            };
        }

        public async Task PublishMessageAsync(CompetingConsumersMessage message)
        {
            await OpenConnectionAndPublishAsync(message);
        }

        private async Task OpenConnectionAndPublishAsync(CompetingConsumersMessage message)
        {
            await using var connection =
                await _connectionFactory.CreateConnectionAsync();

            await using var channel =
                await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                 queue: _rabbitMQConfiguration.QueueName,
                 durable: true,
                 exclusive: false,
                 autoDelete: false,
                 arguments: null);

            for (int i = 0; i < 10; i++)
            {
                message.Id = i + 1;
                var body = JsonSerializer.SerializeToUtf8Bytes(message);

                await channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: _rabbitMQConfiguration.QueueName,
                    body: body);
            }
        }
    }
}