using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace DirctExchange.Consumer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var factory = new ConnectionFactory
            {
                HostName = "host.docker.internal",
                Port = 5672
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            // اسم Queue ثابت
            var queueName = "qu-notification";

            // Declare Queue
            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            // =========================
            // Direct Exchange (amq.direct)
            // =========================
            var routingKeys = args.Length == 0
                ? new[] { "key-notification" }
                : args;

            foreach (var routingKey in routingKeys)
            {
                await channel.QueueBindAsync(
                    queue: queueName,
                    exchange: "amq.direct",
                    routingKey: routingKey
                );
            }

            // =========================
            // Fanout Exchange (amq.fanout)
            // =========================
            // Fanout ما بياخدش routing key
            await channel.QueueBindAsync(
                queue: queueName,
                exchange: "amq.fanout",
                routingKey: string.Empty
            );

            Console.WriteLine(" [*] Waiting for messages (Direct + Fanout). To exit press CTRL+C");

            // Consumer
            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                Console.WriteLine($" [x] Received '{ea.RoutingKey}':'{message}'");

                await Task.CompletedTask;
            };

            await channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: true,
                consumer: consumer
            );

            await Task.Delay(Timeout.Infinite);
        }
    }
}
