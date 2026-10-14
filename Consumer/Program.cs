
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Configurations;
using Shared.Messages;
using System.Text.Json;

namespace Consumer
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

            channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);
            await channel.QueueDeclareAsync(
                 queue: rabbitMQConfig.QueueName,
                 durable: true,
                 exclusive: false,
                 autoDelete: false,
                 arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            var random = new Random();
            consumer.ReceivedAsync += async (_, eventArgs) =>
            {
                try
                {
                    var processingTime = random.Next(1, 6);
                    // 1. Read the message body
                    var body = eventArgs.Body.ToArray();

                    // 2. Deserialize JSON into CompetingConsumersMessage
                    var message = JsonSerializer.Deserialize<CompetingConsumersMessage>(body);

                    // 3. Print message properties
                    if (message is not null)
                    {
                        Console.WriteLine("\n========== Received Message ==========");
                        Console.WriteLine($"Id      : {message.Id}");
                        Console.WriteLine($"Content : {message.Content}");
                        Console.WriteLine("======================================\n");
                        Console.WriteLine($"Simulating processing for {processingTime} seconds...");
                        Console.WriteLine("======================================\n");
                        Thread.Sleep(TimeSpan.FromSeconds(processingTime));

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
                queue: rabbitMQConfig.QueueName,
                autoAck: false,
                consumer: consumer);

            Console.WriteLine(
                $"Listening on queue: {rabbitMQConfig.QueueName}");

            await Task.Delay(Timeout.Infinite);
        }
    }
}

//using Microsoft.Extensions.Configuration;
//using RabbitMQ.Client;
//using RabbitMQ.Client.Events;
//using Shared.Configurations;
//using Shared.Messages;
//using System.Text.Json;

//namespace Consumer
//{
//    public class Program
//    {
//        static async Task Main(string[] args)
//        {
//            await ConfigureServicesAsync();
//        }

//        private static async Task ConfigureServicesAsync()
//        {
//            IConfiguration configuration = new ConfigurationBuilder()
//                .SetBasePath(AppContext.BaseDirectory)
//                .AddJsonFile(
//                    "appsettings.json",
//                    optional: false,
//                    reloadOnChange: true)
//                .Build();

//            var rabbitMQConfig = configuration
//                .GetSection("RabbitMQ")
//                .Get<RabbitMQConfiguration>()
//                ?? throw new InvalidOperationException(
//                    "RabbitMQ configuration is missing.");

//            var factory = new ConnectionFactory
//            {
//                HostName = rabbitMQConfig.Server,
//                Port = rabbitMQConfig.Port,
//                UserName = rabbitMQConfig.UserName,
//                Password = rabbitMQConfig.Password,
//                VirtualHost = rabbitMQConfig.VirtualHost
//            };

//            await using var connection =
//                await factory.CreateConnectionAsync();

//            await using var channel =
//                await connection.CreateChannelAsync();

//            await channel.QueueDeclareAsync(
//                 queue: rabbitMQConfig.QueueName,
//                 durable: true,
//                 exclusive: false,
//                 autoDelete: false,
//                 arguments: null);

//            var consumer = new AsyncEventingBasicConsumer(channel);

//            consumer.ReceivedAsync += async (_, eventArgs) =>
//            {
//                try
//                {
//                    // 1. Read the message body
//                    var body = eventArgs.Body.ToArray();

//                    // 2. Deserialize JSON into CompetingConsumersMessage
//                    var message = JsonSerializer.Deserialize<CompetingConsumersMessage>(body);

//                    // 3. Print message properties
//                    if (message is not null)
//                    {
//                        Console.WriteLine("\n========== Received Message ==========");
//                        Console.WriteLine($"Title   : {message.Title}");
//                        Console.WriteLine($"Content : {message.Content}");
//                        Console.WriteLine($"From    : {message.From}");
//                        Console.WriteLine($"To      : {message.To}");
//                        Console.WriteLine("======================================\n");
//                    }

//                    // 4. Acknowledge successful processing
//                    await channel.BasicAckAsync(
//                        deliveryTag: eventArgs.DeliveryTag,
//                        multiple: false);
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"Error processing message: {ex.Message}");

//                    await channel.BasicNackAsync(
//                        deliveryTag: eventArgs.DeliveryTag,
//                        multiple: false,
//                        requeue: false);
//                }
//            };

//            await channel.BasicConsumeAsync(
//                queue: rabbitMQConfig.QueueName,
//                autoAck: false,
//                consumer: consumer);

//            Console.WriteLine(
//                $"Listening on queue: {rabbitMQConfig.QueueName}");

//            await Task.Delay(Timeout.Infinite);
//        }
//    }
//}
