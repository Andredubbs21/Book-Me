using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using RabbitMQ.Client;

namespace Mproducer.iMessage.services;

public class MessageProducer : IMessageProducer
{

    public void SendingMessage<T>(T message)
    {

        var factory = new ConnectionFactory()
        {
            HostName ="localhost",
            UserName = "user",
            Password= "Pass",
            VirtualHost="/" 
        };

        var conn = factory.CreateConnection();  

        using var channel = conn.CreateModel();

        channel.QueueDeclare("Notificacion", durable: true, exclusive: true);

        var jsonString = JsonSerializer.Serialize(message);
        var body= Encoding.UTF8.GetBytes(jsonString);
        channel.BasicPublish("","Notificacion",body:body);
        


    }
}    