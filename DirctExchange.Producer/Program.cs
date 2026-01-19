using RabbitMQ.Client;
using System.Text;

namespace DirctExchange.Producer
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

            // amq.direct Exchange موجود مسبقًا → لا تعلن عنه
            var severity = args.Length > 0 ? args[0] : "key-notification";
            var message = args.Length > 1
                ? string.Join(" ", args.Skip(1))
                : "Hello World!";

            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(
                exchange: "amq.direct",
                routingKey: severity,
                body: body
            );

            Console.WriteLine($" [x] Sent '{severity}':'{message}'");
            Console.ReadLine();
        }
    }
}
