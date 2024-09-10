using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net.Security;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using FluentEmail.Core;
using NotificationService.Services;
using NotificationService.Dtos.Event;
using System.Text.Json;
using NotificationService.Dto.booking;
using System.Net.Http.Json;
using NotificationService.Dto;



namespace NotificationService;

public class Worker : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _bookingQueue;
    private readonly IModel _cancelBookingQueue;
    private readonly EventingBasicConsumer _createConsumer;
    private readonly EventingBasicConsumer _cancelConsumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Worker> _logger;


    private readonly IHttpClientFactory _clientFactory;
    private const string UserRoute = "http://localhost:5253/api/user";
    private const string EventRoute = "http://localhost:5059/api/event";

    public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory, IHttpClientFactory clientFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _clientFactory = clientFactory;

        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "user",
            Password = "mypasss"
        };

        _connection = factory.CreateConnection();
        _bookingQueue = _connection.CreateModel();
        _bookingQueue.QueueDeclare(queue: "bookingQueue",
                              durable: false,
                              exclusive: false,
                              autoDelete: false,
                              arguments: null);

        _cancelBookingQueue = _connection.CreateModel();
        _cancelBookingQueue.QueueDeclare(queue: "cancelBookingQueue",
                              durable: false,
                              exclusive: false,
                              autoDelete: false,
                              arguments: null);

        _createConsumer = new EventingBasicConsumer(_bookingQueue);
        _cancelConsumer = new EventingBasicConsumer(_cancelBookingQueue);

    }

    private static SemaphoreSlim _semaphore = new SemaphoreSlim(5); // Permitir hasta 5 correos simultáneos

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _createConsumer.Received += async (model, content) =>
        {
            await _semaphore.WaitAsync(); // Limitar conexiones simultáneas

            try
            {
                var email = "";
                var eventName = "";
                var body = content.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(message);

                var createBooking = JsonSerializer.Deserialize<CreateBookingDto>(message);
                if (createBooking != null)
                {
                    Console.WriteLine($"Respuesta User {createBooking}");
                    
                    var client = _clientFactory.CreateClient();
                    var eventResponse = await client.GetAsync($"{EventRoute}/{createBooking.EventId}");
                    Console.WriteLine($"Respuesta User {eventResponse}");
                    var eventDetails = await eventResponse.Content.ReadFromJsonAsync<EventDetailsDto>();
                    if (eventDetails != null)
                    {
                        Console.WriteLine(eventDetails);
                        eventName = eventDetails.Name;
                    }

                    var userResponse = await client.GetAsync($"{UserRoute}/user_name/{createBooking.Username}");
                    Console.WriteLine($"Respuesta User {userResponse}");
                    var userDetails = await userResponse.Content.ReadFromJsonAsync<UserDetailsDto>();
                    if (userDetails != null)
                    {
                        Console.WriteLine(userDetails);
                        email = userDetails.Email;
                    }

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                        var emailMetadata = new EmailMetadata(
                            toAddress: email,
                            subject: "Se ha registrado a un evento",
                            body: $"{createBooking.Username} ha hecho una reservacion al evento {eventName}.Este es un mensaje automatico No necesita responderlo."
                        );

                        await emailService.Send(emailMetadata);
                    }
                }
                _bookingQueue.BasicAck(content.DeliveryTag, false);
            }
            finally
            {
                _semaphore.Release(); // Liberar el recurso
            }
        };

        _cancelConsumer.Received += async (model, content) =>
        {
            await _semaphore.WaitAsync(); // Limitar conexiones simultáneas

            try
            {
                var body = content.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(message);




                //var baseMessage = JsonConverter.Deserialize<BaseMessage>(message);
                //var eventDetails = await eventResponse.Content.ReadFromJsonAsync<EventDetailsDto>();

                //var client = _clientFactory.CreateClient();
                //var eventResponse = await client.GetAsync($"{EventRoute}/{message.EventId}");
                //var eventDetails = await eventResponse.Content.ReadFromJsonAsync<EventDetailsDto>(message);

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

                _cancelBookingQueue.BasicAck(content.DeliveryTag, false);
            }
            finally
            {
                _semaphore.Release(); // Liberar el recurso
            }
        };

        _bookingQueue.BasicConsume(queue: "bookingQueue", autoAck: false, consumer: _createConsumer);
        _cancelBookingQueue.BasicConsume(queue: "cancelBookingQueue", autoAck: false, consumer: _cancelConsumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override void Dispose()
    {
        _bookingQueue.Close();
        _cancelBookingQueue.Close();
        _connection.Close();
        base.Dispose();
    }
}
