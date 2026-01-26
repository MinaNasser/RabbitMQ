using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace DirctExchange.Producer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //var factory = new ConnectionFactory
            //{
            //    HostName = "host.docker.internal",
            //    Port = 5672
            //};

            //await using var connection = await factory.CreateConnectionAsync();
            //await using var channel = await connection.CreateChannelAsync();

            //// الرسالة
            //var severity = args.Length > 0 ? args[0] : "key-notification";
            //var message = args.Length > 1
            //    ? string.Join(" ", args.Skip(1))
            //    : "Hello World!";

            //var body = Encoding.UTF8.GetBytes(message);

            //// 1️⃣ إرسال Direct
            //await channel.BasicPublishAsync(
            //    exchange: "amq.direct",
            //    routingKey: severity,
            //    body: body
            //);
            //Console.WriteLine($" [x] Sent Direct '{severity}':'{message}'");

            //// 2️⃣ إرسال Fanout
            //await channel.BasicPublishAsync(
            //    exchange: "amq.fanout",
            //    routingKey: string.Empty,
            //    body: body
            //);
            //Console.WriteLine($" [x] Sent to Fanout: '{message}'");

            //// ========================
            //// Example: Consumer for Fanout Queues
            //// ========================
            //string[] fanoutQueues = { "q.fanout1", "q.fanout2", "q.fanout3", "q.fanout4", "q.fanout5" };

            //foreach (var queueName in fanoutQueues)
            //{
            //    // Declare each queue
            //    await channel.QueueDeclareAsync(
            //        queue: queueName,
            //        durable: true,
            //        exclusive: false,
            //        autoDelete: false
            //    );

            //    // Bind to fanout exchange
            //    await channel.QueueBindAsync(
            //        queue: queueName,
            //        exchange: "amq.fanout",
            //        routingKey: string.Empty
            //    );

            //    // Consumer
            //    var consumer = new AsyncEventingBasicConsumer(channel);
            //    consumer.ReceivedAsync += async (model, ea) =>
            //    {
            //        var bodyReceived = ea.Body.ToArray();
            //        var msgReceived = Encoding.UTF8.GetString(bodyReceived);
            //        Console.WriteLine($" [x] {queueName} Received: '{msgReceived}'");
            //        await Task.CompletedTask;
            //    };

            //    await channel.BasicConsumeAsync(
            //        queue: queueName,
            //        autoAck: true,
            //        consumer: consumer
            //    );
            //}

            //// Keep running
            //Console.WriteLine("Press CTRL+C to exit.");
            //await Task.Delay(Timeout.Infinite);


            //var factory = new ConnectionFactory
            //{
            //    HostName = "localhost"
            //};
            //await using var connection = await factory.CreateConnectionAsync();
            //await using var channel = await connection.CreateChannelAsync();
            //Console.WriteLine("Connection established.");
            //const string message = "Test Topic Exchange ";
            //var body = Encoding.UTF8.GetBytes(message);
            //await channel.BasicPublishAsync(
            //    exchange: "amq.topic",
            //    routingKey: "h.y",
            //    body: body
            //);
            //Console.WriteLine($" [x] Sent Topic '{message}'");
            //Console.WriteLine($"Press [Enter ] to Exit .");
            //Console.ReadLine();

            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            Console.WriteLine("Connection established.");

            var properties = new BasicProperties
            {
                Persistent = false,
                Headers = new Dictionary<string, object>
                {
                    { "name", "info" }
                }
            };

            const string message = "Test Headers Exchange";
            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(
                exchange: "amq.headers",
                routingKey: string.Empty,
                mandatory: false,
                basicProperties: properties,
                body: body
            );

            Console.WriteLine($" [x] Sent Headers '{message}'");
            Console.WriteLine("Press [Enter] to Exit.");
            Console.ReadLine();

        }
    }
}
