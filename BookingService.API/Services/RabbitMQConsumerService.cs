namespace BookingService.API.Services;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using BookingService.API.Dto.booking;
using BookingService.API.Data;
using Microsoft.EntityFrameworkCore;

public class RabbitMQCancelConsumerService : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly BookingDbContext _dbContext;

    public RabbitMQCancelConsumerService(BookingDbContext dbContext)
    {
        _dbContext = dbContext;

        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        _channel.QueueDeclare(queue: "cancelBookingQueue",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var cancelBookingDto = JsonSerializer.Deserialize<CancelBookingDto>(message);

            if (cancelBookingDto != null)
            {
                // Procesa el mensaje para cancelar la reserva
                await HandleCancelBooking(cancelBookingDto);
            }

            Console.WriteLine($" [x] Received cancellation request for booking ID {cancelBookingDto?.Id}");
        };

        _channel.BasicConsume(queue: "cancelBookingQueue",
                             autoAck: true,
                             consumer: consumer);

        return Task.CompletedTask;
    }

    private async Task HandleCancelBooking(CancelBookingDto cancelBookingDto)
    {
        var booking = await _dbContext.Bookings
            .FirstOrDefaultAsync(b => b.Id == cancelBookingDto.Id && b.Username == cancelBookingDto.Username && b.EventId == cancelBookingDto.EventId);

        if (booking != null)
        {
            _dbContext.Bookings.Remove(booking);
            await _dbContext.SaveChangesAsync();
            Console.WriteLine($" [x] Booking ID {cancelBookingDto.Id} canceled successfully.");
        }
        else
        {
            Console.WriteLine($" [x] Booking ID {cancelBookingDto.Id} not found or mismatch.");
        }
    }

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}
