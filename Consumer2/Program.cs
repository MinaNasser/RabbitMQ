
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Configurations;
using Shared.Messages;
using System.Text.Json;

namespace Consumer2
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            await ConfigureServicesAsync();
        }

        private static async Task ConfigureServicesAsync()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(
                    "appsettings.json",
                    optional: false,
                    reloadOnChange: true)
                .Build();

            var rabbitMQConfig = configuration
                .GetSection("RabbitMQ")
                .Get<RabbitMQConfiguration>()
                ?? throw new InvalidOperationException(
                    "RabbitMQ configuration is missing.");

            var factory = new ConnectionFactory
            {
                HostName = rabbitMQConfig.Server,
                Port = rabbitMQConfig.Port,
                UserName = rabbitMQConfig.UserName,
                Password = rabbitMQConfig.Password,
                VirtualHost = rabbitMQConfig.VirtualHost
            };

            await using var connection =
                await factory.CreateConnectionAsync();

            await using var channel =
                await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
             exchange: rabbitMQConfig.ExchangeName,
             //type: ExchangeType.Fanout,
             type: ExchangeType.Direct,
             durable: true,
             autoDelete: false,
             arguments: null);

            var queueNameDeclared = await channel.QueueDeclareAsync();

            var queueName = queueNameDeclared.QueueName;

            await channel.QueueBindAsync(
                queue: queueName,
                exchange: rabbitMQConfig.ExchangeName,
                //routingKey: ""
                routingKey: rabbitMQConfig.RoutingKey
                );
            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, eventArgs) =>
            {
                try
                {

                    var body = eventArgs.Body.ToArray();

                    // 2. Deserialize JSON into FirstDemoMessage
                    var message = JsonSerializer.Deserialize<FirstDemoMessage>(body);

                    // 3. Print message properties
                    if (message is not null)
                    {
                        Console.WriteLine("Consumer 2");
                        Console.WriteLine("\n========== Received Message ==========");
                        Console.WriteLine($"Title   : {message.Title}");
                        Console.WriteLine($"Content : {message.Content}");
                        Console.WriteLine($"From    : {message.From}");
                        Console.WriteLine($"To      : {message.To}");
                        Console.WriteLine("======================================\n");

                    }

                    // 4. Acknowledge successful processing
                    await channel.BasicAckAsync(
                        deliveryTag: eventArgs.DeliveryTag,
                        multiple: false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");

                    await channel.BasicNackAsync(
                        deliveryTag: eventArgs.DeliveryTag,
                        multiple: false,
                        requeue: false);
                }
            };

            await channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer);

            Console.WriteLine(
                $"Consumer 2 Listening on queue: {queueName}");

            await Task.Delay(Timeout.Infinite);
        }
    }
}
