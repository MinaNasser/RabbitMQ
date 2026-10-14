
using RabbitMQ.Client;
using Shared.Configurations;
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

        public async Task PublishMessageAsync<FirstDemoMessage>(FirstDemoMessage message)
        {
            await OpenConnectionAndPublishAsync(message);
        }

        private async Task OpenConnectionAndPublishAsync<FirstDemoMessage>(FirstDemoMessage message)
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


            var body = JsonSerializer.SerializeToUtf8Bytes(message);

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _rabbitMQConfiguration.QueueName,
                body: body);
        }
    }
}
