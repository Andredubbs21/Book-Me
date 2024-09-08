using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;
namespace NotificationService;

public class Worker : BackgroundService
{
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly EventingBasicConsumer _consumer;

    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "user",  // Usuario definido en docker-compose
            Password = "mypasss"  // Contraseña definida en docker-compose
        };
         _connection = factory.CreateConnection();
         _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: "bookingQueue",
                                durable: false,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);

        var _consumer = new EventingBasicConsumer(_channel);
    }

  public override Task StartAsync(CancellationToken cancellationToken)
        {
          _consumer.Received += (model, content) => {
                var body = content.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(message);
            };

            _channel.BasicConsume(queue: "bookingQueue",
                                  autoAck: false,
                                  consumer: _consumer);

            return Task.CompletedTask;
        }


}
