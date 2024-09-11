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
using NotificationService.API.Dto.Rabbit;



namespace NotificationService;

public class Worker : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _bookingQueue;
    //private readonly IModel _cancelBookingQueue;
    private readonly EventingBasicConsumer _createConsumer;
    //private readonly EventingBasicConsumer _cancelConsumer;
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
        /*
        _cancelBookingQueue = _connection.CreateModel();
        _cancelBookingQueue.QueueDeclare(queue: "cancelBookingQueue",
                              durable: false,
                              exclusive: false,
                              autoDelete: false,
                              arguments: null);
                              */

        _createConsumer = new EventingBasicConsumer(_bookingQueue);
        //_cancelConsumer = new EventingBasicConsumer(_cancelBookingQueue);

    }

    private static SemaphoreSlim _semaphore = new SemaphoreSlim(5); // Permitir hasta 5 correos simultáneos

    public bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _createConsumer.Received += async (model, content) =>
        {
            await _semaphore.WaitAsync(); // Limitar conexiones simultáneas

            try
            {
                var body = content.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine("Se viene el mensaje de prueba");
                Console.WriteLine(message);
                var createBooking = JsonSerializer.Deserialize<messageInfo>(message);
                Console.WriteLine(createBooking);

                if (createBooking != null)
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                        if (IsValidEmail(createBooking.email))
                        {
                            if(createBooking.crear){
                                var emailMetadata = new EmailMetadata(
                                toAddress: createBooking.email,
                                subject: "Se ha registrado a un evento",
                                body: $"{createBooking.userName} ha hecho una reservacion al evento {createBooking.eventName}.Este es un mensaje automatico No necesita responderlo."
                                );
                                    await emailService.Send(emailMetadata);//lo coloque aqui tambien
                            }else{
                                var emailMetadata = new EmailMetadata(
                                toAddress: createBooking.email,
                                subject: "Ha cancelado un evento",
                                body: $"{createBooking.userName} se ha cancelado su reservacion a este evento {createBooking.eventName}.Este es un mensaje automatico No necesita responderlo."
                                );
                                    await emailService.Send(emailMetadata);// lo coloque aqui 
                            }                            

                           // await emailService.Send(emailMetadata);  <---- Este es el error
                        }else{
                           
                            _logger.LogWarning($"La dirección de correo {createBooking.email} no es válida. No se enviará el correo.");
                        }
                         _bookingQueue.BasicAck(content.DeliveryTag, false);
                    }

                }

                /*
                if(createBooking != null){
                    Console.WriteLine($"User: {createBooking.Username}, Evento: {createBooking.EventId}");

                    var client = _clientFactory.CreateClient();
                    var eventResponse = await client.GetAsync($"{EventRoute}/{createBooking.EventId}");

                    if(eventResponse.IsSuccessStatusCode){

                        Console.WriteLine("funco");
                        var eventDetails = await eventResponse.Content.ReadFromJsonAsync<EventDetailsDto>();
                        Console.WriteLine("Miren datos", eventDetails);
                        if(eventDetails != null) {
                            Console.WriteLine("Miren datos", eventDetails!.Name);
                            
                        }
                        
                    }
                }
                /*
                
                /*
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

                    
                    
                }
                */

            }
            finally
            {
                _semaphore.Release(); // Liberar el recurso
            }
        };
        /*
        _cancelConsumer.Received += async (model, content) =>
        {
            await _semaphore.WaitAsync(); // Limitar conexiones simultáneas

            try
            {
                var body = content.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine("Se viene el mensaje de prueba");
                Console.WriteLine(message);
                var cancelBooking = JsonSerializer.Deserialize<messageInfo>(message);
                Console.WriteLine(cancelBooking);

                if (cancelBooking != null)
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                        if (IsValidEmail(cancelBooking.email))
                        {
                            var emailMetadata = new EmailMetadata(
                                toAddress: cancelBooking.email,
                                subject: "Ha cancelado un evento",
                                body: $"{cancelBooking.userName} se ha cancelado su reservacion a este evento {cancelBooking.eventName}.Este es un mensaje automatico No necesita responderlo."
                            );

                            await emailService.Send(emailMetadata);
                        }else{
                           
                            _logger.LogWarning($"La dirección de correo {cancelBooking.email} no es válida. No se enviará el correo.");
                        }
                         _cancelBookingQueue.BasicAck(content.DeliveryTag, false);
                    }

                }



                //var baseMessage = JsonConverter.Deserialize<BaseMessage>(message);
                //var eventDetails = await eventResponse.Content.ReadFromJsonAsync<EventDetailsDto>();

                //var client = _clientFactory.CreateClient();
                //var eventResponse = await client.GetAsync($"{EventRoute}/{message.EventId}");
                //var eventDetails = await eventResponse.Content.ReadFromJsonAsync<EventDetailsDto>(message);

                

                //_cancelBookingQueue.BasicAck(content.DeliveryTag, false);
            }
            finally
            {
                _semaphore.Release(); // Liberar el recurso
            }
        };
        */

        _bookingQueue.BasicConsume(queue: "bookingQueue", autoAck: false, consumer: _createConsumer);
        //_cancelBookingQueue.BasicConsume(queue: "cancelBookingQueue", autoAck: false, consumer: _cancelConsumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override void Dispose()
    {
        _bookingQueue.Close();
        //_cancelBookingQueue.Close();
        _connection.Close();
        base.Dispose();
    }
}
