using BookingService.API.Mapping;
using BookingService.API.Dto.booking;
using BookingService.API.Data;
using Microsoft.EntityFrameworkCore;
using BookingService.API.Dto;
using BookingService.Api.Dtos.Event;
using BookingService.API.Entities;
namespace BookingService.API.Endpoints;
public static class BookingEndpoints
{
    public static RouteGroupBuilder MapBookingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("bookings").WithParameterValidation();
        const string getBookingEndpoint = "GetBookingById"; 
        const string userRoute = "http://localhost:5253/users";
        const string eventRoute = "http://localhost:5059/events";
        // Get Bookings
        group.MapGet("/", async (BookingDbContext dbContext) =>
            await dbContext.Bookings
                // Transforms bookings to summarized dto versions
                .Select(booking => booking.ToBookingSummaryDto())
                // Removes tracking for better performance (optional)
                .AsNoTracking()
                // Makes list out of the query 
                .ToListAsync());

        // Get bookings by id
        group.MapGet("/{id}", async (int id, BookingDbContext dbContext, IHttpClientFactory clientFactory) =>{
            // Search for bookings by id. result may be null 
            Booking? booking = await dbContext.Bookings.FindAsync(id);
            if (booking is null){
                return Results.NotFound();
            }
            BookingDetailsDto? detailedBooking = booking!.ToBookingDetailsDto();
            var client = clientFactory.CreateClient();
            // Search for event by id, result may be null 
            var eventResponse = await client.GetAsync($"{eventRoute}/{id}");
            if (!eventResponse.IsSuccessStatusCode)
            {
                return Results.NotFound("Event not found");
            }
            var eventDetails = await eventResponse.Content.ReadFromJsonAsync<EventDetailsDto>();
            detailedBooking.EventName = eventDetails!.Name;
            // Return either null or a detailed version of the booking
            return booking is null ? Results.NotFound() : Results.Ok(detailedBooking);
        }).WithName(getBookingEndpoint);

        // Get bookings by username 
        group.MapGet("/user_name/{username}", async (string username, BookingDbContext dbContext,IHttpClientFactory clientFactory ) =>
        {
            // Search for bookings by event id. result will be a list
            var bookings = await dbContext.Bookings
                .AsNoTracking()
                .Where(b => b.Username == username)
                .Select(b => b.ToBookingDetailsDto())
                .ToListAsync();
            foreach (var booking in bookings)
            {
                 var client = clientFactory.CreateClient();
                // Search for event by id, result may be null 
                var eventResponse = await client.GetAsync($"{eventRoute}/{booking.EventId}");
                if (!eventResponse.IsSuccessStatusCode){
                    booking.EventName = "";
                }
                else{
                    var eventDetails = await eventResponse.Content.ReadFromJsonAsync<EventDetailsDto>();
                    booking.EventName = eventDetails!.Name;
                }   
            }
            // Return the list of bookings
            return bookings.Count == 0 ? Results.NotFound() : Results.Ok(bookings);
        }).WithName("GetBookingByUsername");
        
        // Get bookings by event id 
        group.MapGet("/event_id/{id}", async (int id, BookingDbContext dbContext, IHttpClientFactory clientFactory) =>{
            // Search for bookings by event id. result will be a list
            var bookings = await dbContext.Bookings
                .AsNoTracking()
                .Where(b => b.EventId == id)
                .Select(b => b.ToBookingDetailsDto())
                .ToListAsync();
            foreach (var booking in bookings)
            {
                var client = clientFactory.CreateClient();
                // Search for event by id, result may be null 
                var eventResponse = await client.GetAsync($"{eventRoute}/{booking.EventId}");
                if (!eventResponse.IsSuccessStatusCode){
                    booking.EventName = "";
                }
                else{
                    var eventDetails = await eventResponse.Content.ReadFromJsonAsync<EventDetailsDto>();
                    booking.EventName = eventDetails!.Name;
                }   
            }
            // Return the list of bookings
            return bookings.Count == 0 ? Results.NotFound() : Results.Ok(bookings);
        }).WithName("GetBookingByEventId");

        // Create booking
        group.MapPost("/", async(CreateBookingDto newBooking, BookingDbContext dbContext, IHttpClientFactory clientFactory) => {
            var client = clientFactory.CreateClient();
            // verify user exists
            var userResponse = await client.GetAsync($"{userRoute}/user_name/{newBooking.Username}");
            if (!userResponse.IsSuccessStatusCode)
            {
                    return Results.NotFound("User not found");
            }
            // verify event exits
            var eventResponse = await client.GetAsync($"{eventRoute}/{newBooking.EventId}");
            if (!eventResponse.IsSuccessStatusCode)
            {
                    return Results.NotFound("Event not found");
            }
            var eventDetails = await eventResponse.Content.ReadFromJsonAsync<EventDetailsDto>();
            // Check event date has not passed 
            var today = DateTime.Now;
                if (today >= eventDetails!.Date){
                    Console.WriteLine("CONCLUDED EVENT");
                    return Results.NoContent();
                }
        
            //CHECK EVENT HAS REMAINING TICKETS 
            var eventBookings = await dbContext.Bookings
                .AsNoTracking()
                .Where(b => b.EventId == eventDetails.Id)
                .Select(b => b.ToBookingDetailsDto())
                .ToListAsync();
            var totalAmount = eventBookings.Sum(b => b.Amount);
            totalAmount += newBooking.Amount;
            if (totalAmount > eventDetails.MaxCapacity){
                Console.WriteLine("EVENT MAXED OUT");
                return Results.NoContent();
            } 

            //CHECK IF USER HAS NOT ISSUED A TICKET 
            var userBookings = await dbContext.Bookings
                .AsNoTracking()
                .Where(b => b.Username == newBooking.Username && b.EventId == newBooking.EventId)
                .Select(b => b.ToBookingDetailsDto())
                .ToListAsync();
            if (userBookings.Count > 0){
                Console.WriteLine("USER HAS BOOKED ALREADY");
            return Results.NoContent();
            };    
            // Convert dto to entity 
            Booking booking = newBooking.ToEntity();
            // Add user
            dbContext.Bookings.Add(booking);
            // Commit changes
            await dbContext.SaveChangesAsync();
            BookingDetailsDto newBookingDetails = booking.ToBookingDetailsDto();
            newBookingDetails.EventName = eventDetails.Name;
            return Results.CreatedAtRoute(getBookingEndpoint, new { id = booking.Id }, newBookingDetails);
        });

        // Create booking by params 
        group.MapPost("/create_req/{username}/{id}/{amount}", async(string username, int id, int amount, BookingDbContext dbContext, IHttpClientFactory clientFactory) => {
            CreateBookingDto newBooking = new (
                username,
                id, 
                amount
            );
            var client = clientFactory.CreateClient();
            // verify user exists
            var userResponse = await client.GetAsync($"{userRoute}/user_name/{newBooking.Username}");
            if (!userResponse.IsSuccessStatusCode)
            {
                    return Results.NotFound("User not found");
            }
            // verify event exits
            var eventResponse = await client.GetAsync($"{eventRoute}/{newBooking.EventId}");
            if (!eventResponse.IsSuccessStatusCode)
            {
                    return Results.NotFound("Event not found");
            }
            var eventDetails = await eventResponse.Content.ReadFromJsonAsync<EventDetailsDto>();
            // Check event date has not passed 
            var today = DateTime.Now;
                if (today >= eventDetails!.Date){
                    Console.WriteLine("CONCLUDED EVENT");
                    return Results.NoContent();
                }
        
            //CHECK EVENT HAS REMAINING TICKETS 
            var eventBookings = await dbContext.Bookings
                .AsNoTracking()
                .Where(b => b.EventId == eventDetails.Id)
                .Select(b => b.ToBookingDetailsDto())
                .ToListAsync();
            var totalAmount = eventBookings.Sum(b => b.Amount);
            totalAmount += newBooking.Amount;
            if (totalAmount > eventDetails.MaxCapacity){
                Console.WriteLine("EVENT MAXED OUT");
                return Results.NoContent();
            } 

            //CHECK IF USER HAS NOT ISSUED A TICKET 
            var userBookings = await dbContext.Bookings
                .AsNoTracking()
                .Where(b => b.Username == newBooking.Username && b.EventId == newBooking.EventId)
                .Select(b => b.ToBookingDetailsDto())
                .ToListAsync();
            if (userBookings.Count > 0){
            return Results.BadRequest("USER HAS BOOKED ALREADY");
            };    
            // Convert dto to entity 
            Booking booking = newBooking.ToEntity();
            // Add user
            dbContext.Bookings.Add(booking);
            // Commit changes
            await dbContext.SaveChangesAsync();
            BookingDetailsDto newBookingDetails = booking.ToBookingDetailsDto();
            newBookingDetails.EventName = eventDetails.Name;
            return Results.CreatedAtRoute(getBookingEndpoint, new { id = booking.Id }, newBookingDetails);
        });

   
        // Update booking by id 
        group.MapPut("/{id}", async(int id, UpdateBookingDto updatedBooking, BookingDbContext dbContext) =>{
            var existingBooking = await dbContext.Bookings.FindAsync(id);
            if (existingBooking is null){
                return Results.NotFound();
            }
            dbContext.Entry(existingBooking)
            .CurrentValues
            .SetValues(updatedBooking.ToEntity(id));
            await dbContext.SaveChangesAsync();
            return Results.NoContent();
        });

        //  Delete booking by id
        group.MapDelete("/{id}", async(int id, BookingDbContext dbContext) =>{
            await dbContext.Bookings.Where(booking => booking.Id == id).ExecuteDeleteAsync();
            return Results.NoContent();
        });

        // Delete booking by username and event id 
        group.MapDelete("user_event/{username}/{id}", async(int id, string username, BookingDbContext dbContext, IHttpClientFactory clientFactory) =>{
            //check if booking exists
            var booking = await dbContext.Bookings
            .FirstOrDefaultAsync(b => b.Username == username && b.EventId == id);
            if(booking is null){
                Console.WriteLine("Booking not found");
                return Results.NotFound();
            }
            //Validate user exsits
             // Search for user by username. result may be null
            var client = clientFactory.CreateClient();
            var userResponse = await client.GetAsync($"{userRoute}/user_name/{username}");
            if (!userResponse.IsSuccessStatusCode)
            {
                    return Results.NotFound("User not found");
            }
            // verify event exits
            var eventResponse = await client.GetAsync($"{eventRoute}/{booking.EventId}");
            if (!eventResponse.IsSuccessStatusCode)
            {
                return Results.NotFound("Event not found");
            }
            // CHECK EVENT DATE HAS NOT PASSED
            var eventDetails = await eventResponse.Content.ReadFromJsonAsync<EventDetailsDto>();
            var today = DateTime.Now;
            if (today >= eventDetails!.Date){
                Console.WriteLine("CONCLUDED EVENT");
                return Results.NoContent();
            }
            dbContext.Bookings.Remove(booking);
            await dbContext.SaveChangesAsync();
            return Results.NoContent();
        }); 
        return group;
    }

}