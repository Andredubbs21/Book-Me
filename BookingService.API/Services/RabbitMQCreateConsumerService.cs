namespace BookingService.API.Services;

using BookingService.API.Dto.booking;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class RabbitMQConsumerService : BackgroundService
{
    private IConnection _connection;
    private IModel _channel;

    public RabbitMQConsumerService()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: "bookingQueue",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            // Deserializar el mensaje usando System.Text.Json
            var booking = JsonSerializer.Deserialize<CreateBookingDto>(message);

            // Procesar el mensaje (ejemplo: imprimirlo en consola)
            Console.WriteLine($" [x] Received {message}");

            // Aquí puedes realizar cualquier otra lógica con los datos del booking
            // Ejemplo: almacenar en base de datos, enviar correos, etc.
        };

        _channel.BasicConsume(queue: "bookingQueue",
                             autoAck: true,
                             consumer: consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}