using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net.Security;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using FluentEmail.Core;
using NotificationService.Services;


namespace NotificationService;

public class Worker : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly EventingBasicConsumer _consumer;  // Aquí no necesita ser redeclarado
    private readonly IEmailService _emailService;

    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger, IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;

        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "user",
            Password = "mypasss" 
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: "bookingQueue",
                                durable: false,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);

        _consumer = new EventingBasicConsumer(_channel);

    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _consumer.Received += (model, content) =>
        {
            var body = content.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine(message);
            
            var emailMetadata = new EmailMetadata(
                toAddress: "edgar.r.0228@gmail.com",
                subject: "Prueba#1",
                body: "Sos un Crack, lograste hacer lo de los emails."
            );

            _channel.BasicAck(content.DeliveryTag, false);
        };


        _channel.BasicConsume(queue: "bookingQueue",
                              autoAck: false,
                              consumer: _consumer);

        return Task.CompletedTask;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}
