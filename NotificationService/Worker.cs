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
    private readonly EventingBasicConsumer _consumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;

        try
        {
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

            _logger.LogInformation("Connected to RabbitMQ successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to connect to RabbitMQ: {ex.Message}");
            throw;
        }
    }

    private static SemaphoreSlim _semaphore = new SemaphoreSlim(5); // Permitir hasta 5 correos simultáneos

protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    _consumer.Received += async (model, content) =>
    {
        await _semaphore.WaitAsync(); // Limitar conexiones simultáneas

        try
        {
            var body = content.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine(message);

            using (var scope = _scopeFactory.CreateScope())
            {
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var emailMetadata = new EmailMetadata(
                    toAddress: "edgar.r.0228@gmail.com",
                    subject: "Prueba#1",
                    body: "Sos un Crack, lograste hacer lo de los emails."
                );

                await emailService.Send(emailMetadata);
            }

            _channel.BasicAck(content.DeliveryTag, false);
        }
        finally
        {
            _semaphore.Release(); // Liberar el recurso
        }
    };

    _channel.BasicConsume(queue: "bookingQueue", autoAck: false, consumer: _consumer);

    while (!stoppingToken.IsCancellationRequested)
    {
        await Task.Delay(1000, stoppingToken);
    }
}

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}
