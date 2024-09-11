using Microsoft.AspNetCore.Mvc;
using BookingService.API.Mapping;
using BookingService.API.Data;
using Microsoft.EntityFrameworkCore;
using BookingService.Api.Dtos.Event;
using BookingService.API.Dto.booking;
using BookingService.API.Dto.Rabbit;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookingService.API.Dto;


namespace BookingService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly BookingDbContext _dbContext;
        private readonly IHttpClientFactory _clientFactory;
        private const string UserRoute = "http://localhost:5253/api/user";
        private const string EventRoute = "http://localhost:5059/api/event";


        private void SendCancelBookingToQueue(messageInfo cancelBooking)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "user",  // Usuario definido en docker-compose
                Password = "mypasss"  // Contraseña definida en docker-compose
            };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "cancelBookingQueue",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                string message = JsonSerializer.Serialize(cancelBooking);
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                     routingKey: "cancelBookingQueue",
                                     basicProperties: null,
                                     body: body);

                Console.WriteLine(" [x] Sent cancellation request {0}", message);
            }
        }
        private void SendBookingToQueue(messageInfo newBooking)
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = "localhost",
                    Port = 5672,
                    UserName = "user",  // Usuario definido en docker-compose
                    Password = "mypasss"  // Contraseña definida en docker-compose
                };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "bookingQueue",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    string message = JsonSerializer.Serialize(newBooking);
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: String.Empty,
                                         routingKey: "bookingQueue",
                                         basicProperties: null,
                                         body: body);

                    Console.WriteLine(" [x] Sent {0}", message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                // Registrar excepción o debug
            }
        }
        public BookingController(BookingDbContext dbContext, IHttpClientFactory clientFactory)
        {
            _dbContext = dbContext;
            _clientFactory = clientFactory;
        }

        // GET: api/bookings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingSummaryDto>>> GetBookings()
        {
            var bookings = await _dbContext.Bookings
                .Select(b => b.ToBookingSummaryDto())
                .AsNoTracking()
                .ToListAsync();
            return Ok(bookings);
        }

        // GET: api/bookings/{id}
        [HttpGet("{id}", Name = "GetBookingById")]
        public async Task<ActionResult<BookingDetailsDto>> GetBookingById(int id)
        {
            var booking = await _dbContext.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            var detailedBooking = booking.ToBookingDetailsDto();
            var client = _clientFactory.CreateClient();

            // Fetch event details
            var eventResponse = await client.GetAsync($"{EventRoute}/{booking.EventId}");
            if (!eventResponse.IsSuccessStatusCode)
            {
                return NotFound("Event not found");
            }

            var eventDetails = await eventResponse.Content.ReadFromJsonAsync<EventDetailsDto>();
            detailedBooking.EventName = eventDetails!.Name;
            return Ok(detailedBooking);
        }

        // GET: api/bookings/user_name/{username}
        [HttpGet("user_name/{username}")]
        public async Task<ActionResult<IEnumerable<BookingDetailsDto>>> GetBookingsByUsername(string username)
        {
            var bookings = await _dbContext.Bookings
                .AsNoTracking()
                .Where(b => b.Username == username)
                .Select(b => b.ToBookingDetailsDto())
                .ToListAsync();

            var client = _clientFactory.CreateClient();

            foreach (var booking in bookings)
            {
                var eventResponse = await client.GetAsync($"{EventRoute}/{booking.EventId}");
                if (!eventResponse.IsSuccessStatusCode)
                {
                    booking.EventName = "";
                }
                else
                {
                    var eventDetails = await eventResponse.Content.ReadFromJsonAsync<EventDetailsDto>();
                    booking.EventName = eventDetails!.Name;
                }
            }

            return bookings.Count == 0 ? NotFound() : Ok(bookings);
        }

        //Get bookings by event name
        [HttpGet("event_name/{eventName}")]
        public async Task<ActionResult<IEnumerable<BookingDetailsDto>>> GetBookingsByEventName(string eventName)
        {
            var client = _clientFactory.CreateClient();


            var eventResponse = await client.GetAsync($"{EventRoute}/event_name/{eventName}");
            if (!eventResponse.IsSuccessStatusCode)
            {
                return NotFound();
            }
            var eventDetails = await eventResponse.Content.ReadFromJsonAsync<EventDetailsDto>();
            var eventId = eventDetails!.Id;
            var bookings = await _dbContext.Bookings
                .AsNoTracking()
                .Where(b => b.EventId == eventId)
                .Select(b => b.ToBookingDetailsDto())
                .ToListAsync();

            foreach (var booking in bookings)
            {
                booking.EventName = eventName;
            }

            return bookings.Count == 0 ? NotFound() : Ok(bookings);
        }

        // GET: api/bookings/event_id/{id}
        [HttpGet("event_id/{id}")]
        public async Task<ActionResult<IEnumerable<BookingDetailsDto>>> GetBookingsByEventId(int id)
        {
            var bookings = await _dbContext.Bookings
                .AsNoTracking()
                .Where(b => b.EventId == id)
                .Select(b => b.ToBookingDetailsDto())
                .ToListAsync();

            var client = _clientFactory.CreateClient();

            foreach (var booking in bookings)
            {
                var eventResponse = await client.GetAsync($"{EventRoute}/{booking.EventId}");
                if (!eventResponse.IsSuccessStatusCode)
                {
                    booking.EventName = "";
                }
                else
                {
                    var eventDetails = await eventResponse.Content.ReadFromJsonAsync<EventDetailsDto>();
                    booking.EventName = eventDetails!.Name;
                }
            }

            return bookings.Count == 0 ? NotFound() : Ok(bookings);
        }

        // POST: api/bookings
        [HttpPost]
        public async Task<ActionResult<BookingDetailsDto>> CreateBooking(CreateBookingDto newBooking)
        {
            var client = _clientFactory.CreateClient();

            // Verificar que el usuario existe
            var userResponse = await client.GetAsync($"{UserRoute}/user_name/{newBooking.Username}");
            if (!userResponse.IsSuccessStatusCode)
            {
                return NotFound("User not found");
            }
            
            var userDetails = await userResponse.Content.ReadFromJsonAsync<UserDetailsDto>();
            if(userDetails.Email == ""){
                return BadRequest("No tiene correo existente");
            }

            // Verificar que el evento existe
            var eventResponse = await client.GetAsync($"{EventRoute}/{newBooking.EventId}");
            if (!eventResponse.IsSuccessStatusCode)
            {
                return NotFound("Event not found");
            }

            var eventDetails = await eventResponse.Content.ReadFromJsonAsync<EventDetailsDto>();

            // Verificar que el evento no haya pasado
            if (DateTime.Now >= eventDetails!.Date)
            {
                return BadRequest("Event has already concluded");
            }

            // Verificar la capacidad del evento
            var eventBookings = await _dbContext.Bookings
                .AsNoTracking()
                .Where(b => b.EventId == eventDetails.Id)
                .ToListAsync();
            var totalAmount = eventBookings.Sum(b => b.Amount) + newBooking.Amount;
            if (totalAmount > eventDetails.MaxCapacity)
            {
                return BadRequest("Event capacity exceeded");
            }

            // Verificar si el usuario ya ha emitido un ticket
            var userBookings = await _dbContext.Bookings
                .AsNoTracking()
                .Where(b => b.Username == newBooking.Username && b.EventId == newBooking.EventId)
                .ToListAsync();
            if (userBookings.Count > 0)
            {
                return BadRequest("User has already booked for this event");
            }

            // Convertir DTO a entidad y agregar reserva
            var booking = newBooking.ToEntity();
            _dbContext.Bookings.Add(booking);
            await _dbContext.SaveChangesAsync();

            // Enviar la reserva a la cola de RabbitMQ
            
            
            
            

            // Preparar la respuesta
            var newBookingDetails = booking.ToBookingDetailsDto();
            newBookingDetails.EventName = eventDetails.Name;

            var info = new messageInfo(userDetails.Email,newBooking.Username,eventDetails.Name);

        

            SendBookingToQueue(info);

            return CreatedAtRoute("GetBookingById", new { id = booking.Id }, newBookingDetails);
        }


        [HttpPost("create_req/{username}/{id}/{amount}")]
        public async Task<IActionResult> CreateBookingByParams(string username, int id, int amount)
        {
            var newBooking = new CreateBookingDto(username, id, amount);
            var client = _clientFactory.CreateClient();

            // Verify user exists
            var userResponse = await client.GetAsync($"{UserRoute}/user_name/{newBooking.Username}");
            if (!userResponse.IsSuccessStatusCode)
            {
                return NotFound("User not found");
            }

            // Verify event exists
            var eventResponse = await client.GetAsync($"{EventRoute}/{newBooking.EventId}");
            if (!eventResponse.IsSuccessStatusCode)
            {
                return NotFound("Event not found");
            }

            var eventDetails = await eventResponse.Content.ReadFromJsonAsync<EventDetailsDto>();

            // Check event date has not passed
            if (DateTime.Now >= eventDetails!.Date)
            {
                Console.WriteLine("CONCLUDED EVENT");
                return NoContent();
            }

            // Check event has remaining tickets
            var eventBookings = await _dbContext.Bookings
                .AsNoTracking()
                .Where(b => b.EventId == eventDetails.Id)
                .Select(b => b.ToBookingDetailsDto())
                .ToListAsync();

            var totalAmount = eventBookings.Sum(b => b.Amount) + newBooking.Amount;
            if (totalAmount > eventDetails.MaxCapacity)
            {
                Console.WriteLine("EVENT MAXED OUT");
                return NoContent();
            }

            // Check if user has not issued a ticket
            var userBookings = await _dbContext.Bookings
                .AsNoTracking()
                .Where(b => b.Username == newBooking.Username && b.EventId == newBooking.EventId)
                .Select(b => b.ToBookingDetailsDto())
                .ToListAsync();

            if (userBookings.Count > 0)
            {
                return BadRequest("USER HAS BOOKED ALREADY");
            }

            // Convert dto to entity
            var booking = newBooking.ToEntity();

            // Add booking
            _dbContext.Bookings.Add(booking);

            // Commit changes
            await _dbContext.SaveChangesAsync();

            var newBookingDetails = booking.ToBookingDetailsDto();
            newBookingDetails.EventName = eventDetails.Name;

            return CreatedAtRoute("GetBookingById", new { id = booking.Id }, newBookingDetails);
        }




        [HttpPost("create_req_name/{username}/{eventname}/{amount}")]
        public async Task<IActionResult> CreateBookingByParamsName(string username, string eventname, int amount)
        {
            var client = _clientFactory.CreateClient();

            // Verify event exists
            var eventResponse = await client.GetAsync($"{EventRoute}/event_name/{eventname}");
            if (!eventResponse.IsSuccessStatusCode)
            {
                return NotFound("Event not found");
            }
            var eventDetails = await eventResponse.Content.ReadFromJsonAsync<EventDetailsDto>();
            var newBooking = new CreateBookingDto(username, eventDetails!.Id, amount);
            // Verify user exists
            var userResponse = await client.GetAsync($"{UserRoute}/user_name/{newBooking.Username}");
            if (!userResponse.IsSuccessStatusCode)
            {
                return NotFound("User not found");
            }

            // Check event date has not passed
            if (DateTime.Now >= eventDetails!.Date)
            {
                Console.WriteLine("CONCLUDED EVENT");
                return NoContent();
            }

            // Check event has remaining tickets
            var eventBookings = await _dbContext.Bookings
                .AsNoTracking()
                .Where(b => b.EventId == eventDetails.Id)
                .Select(b => b.ToBookingDetailsDto())
                .ToListAsync();

            var totalAmount = eventBookings.Sum(b => b.Amount) + newBooking.Amount;
            if (totalAmount > eventDetails.MaxCapacity)
            {
                Console.WriteLine("EVENT MAXED OUT");
                return NoContent();
            }

            // Check if user has not issued a ticket
            var userBookings = await _dbContext.Bookings
                .AsNoTracking()
                .Where(b => b.Username == newBooking.Username && b.EventId == newBooking.EventId)
                .Select(b => b.ToBookingDetailsDto())
                .ToListAsync();

            if (userBookings.Count > 0)
            {
                return BadRequest("USER HAS BOOKED ALREADY");
            }

            // Convert dto to entity
            var booking = newBooking.ToEntity();

            // Add booking
            _dbContext.Bookings.Add(booking);

            // Commit changes
            await _dbContext.SaveChangesAsync();

            var newBookingDetails = booking.ToBookingDetailsDto();
            newBookingDetails.EventName = eventDetails.Name;

            return CreatedAtRoute("GetBookingById", new { id = booking.Id }, newBookingDetails);
        }


        // PUT: api/bookings/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(int id, UpdateBookingDto updatedBooking)
        {
            var existingBooking = await _dbContext.Bookings.FindAsync(id);
            if (existingBooking == null)
            {
                return NotFound();
            }

            _dbContext.Entry(existingBooking).CurrentValues.SetValues(updatedBooking.ToEntity(id));
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBookingById(int id)
        {
            var client = _clientFactory.CreateClient();

            var booking = await _dbContext.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            var eventResponse = await client.GetAsync($"{EventRoute}/{booking.EventId}");
            if (!eventResponse.IsSuccessStatusCode)
            {
                return NotFound("Event not found");
            }

            var eventDetails = await eventResponse.Content.ReadFromJsonAsync<EventDetailsDto>();

            // Verificar que el evento no haya pasado

            var cancelBookingDto = new CancelBookingDto
            {
                Id = booking.Id,
                Username = booking.Username,
                EventId = booking.EventId,
            };

            

            var userResponse = await client.GetAsync($"{UserRoute}/user_name/{booking.Username}");
            if (!userResponse.IsSuccessStatusCode)
            {
                return NotFound("User not found");
            }
            
            var userDetails = await userResponse.Content.ReadFromJsonAsync<UserDetailsDto>();
            Console.WriteLine(userDetails);
            if(userDetails.Email == ""){
                return BadRequest("No tiene correo existente");
            }

            // Send cancellation message to RabbitMQ
            var info = new messageInfo(userDetails.Email,booking.Username,eventDetails.Name);

            SendCancelBookingToQueue(info);

            _dbContext.Bookings.Remove(booking);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }


        // DELETE: api/bookings/user_event/{username}/{id}
        [HttpDelete("user_event/{username}/{id}")]
        public async Task<IActionResult> DeleteBookingByUserAndEvent(string username, int id)
        {
            var booking = await _dbContext.Bookings
                .FirstOrDefaultAsync(b => b.Username == username && b.EventId == id);
            if (booking == null)
            {
                return NotFound("Booking not found");
            }

            var client = _clientFactory.CreateClient();

            // Verify user exists
            var userResponse = await client.GetAsync($"{UserRoute}/user_name/{username}");
            if (!userResponse.IsSuccessStatusCode)
            {
                return NotFound("User not found");
            }

            // Verify event exists
            var eventResponse = await client.GetAsync($"{EventRoute}/{id}");
            if (!eventResponse.IsSuccessStatusCode)
            {
                return NotFound("Event not found");
            }

            // Check event date has not passed
            var eventDetails = await eventResponse.Content.ReadFromJsonAsync<EventDetailsDto>();
            if (DateTime.Now >= eventDetails!.Date)
            {
                return BadRequest("Cannot delete booking for concluded event");
            }

            _dbContext.Bookings.Remove(booking);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }


        [HttpDelete("user_eventname/{username}/{eventname}")]
        public async Task<IActionResult> DeleteBookingByUserAndEventName(string username, string eventname)
        {
            var client = _clientFactory.CreateClient();
            // Verify event exists
            var eventResponse = await client.GetAsync($"{EventRoute}/event_name/{eventname}");
            if (!eventResponse.IsSuccessStatusCode)
            {
                return NotFound("Event not found");
            }
            var eventDetails = await eventResponse.Content.ReadFromJsonAsync<EventDetailsDto>();
            var eventId = eventDetails!.Id;
            var booking = await _dbContext.Bookings
                .FirstOrDefaultAsync(b => b.Username == username && b.EventId == eventId);
            if (booking == null)
            {
                return NotFound("Booking not found");
            }

            // Verify user exists
            var userResponse = await client.GetAsync($"{UserRoute}/user_name/{username}");
            if (!userResponse.IsSuccessStatusCode)
            {
                return NotFound("User not found");
            }

            // Check event date has not passed
            if (DateTime.Now >= eventDetails!.Date)
            {
                return BadRequest("Cannot delete booking for concluded event");
            }

            _dbContext.Bookings.Remove(booking);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

    }
}
