using EventService.API.Data;
using EventService.API.Dto;
using EventService.API.Entities;
using EventService.API.Mapping;
using Microsoft.EntityFrameworkCore;


namespace EventService.API.Endpoints;

  public static class EventEndpoints
    {
        public static RouteGroupBuilder MapEventEndpoints(this WebApplication app)
        {
            // Define a group route for the endpoints.
            var group = app.MapGroup("events").WithParameterValidation();
            const string getEventEndpoint = "GetEventById"; 

            // Get Events
            group.MapGet("/", async (EventDbContext dbContext) =>
                await dbContext.Events
                    // Transforms events to summarized dto versions
                    .Select(evt => evt.ToEventSummaryDto())
                    // Removes tracking for better performance (optional)
                    .AsNoTracking()
                    // Makes list out of the query 
                    .ToListAsync());

            // Get events by id
            group.MapGet("/{id}", async (int id, EventDbContext dbContext) =>
            {
                // Search for event by id. result may be null 
                Event? evt = await dbContext.Events.FindAsync(id);
                // Return either null or a detailed version of the event
                return evt is null ? Results.NotFound() : Results.Ok(evt.ToEventDetailsDto());
            }).WithName(getEventEndpoint);

            // Get events by name 
            group.MapGet("/event_name/{name}", async (string name, EventDbContext dbContext) =>
            {
                // Search for event by name. result may be null
                Event? evt = await dbContext.Events
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Name == name);
                // Return either null or a detailed version of the event
                return evt is null ? Results.NotFound() : Results.Ok(evt.ToEventDetailsDto());
            }).WithName("GetEventByName");

            // Create Events
            group.MapPost("/", async (CreateEventDto newEvent, EventDbContext dbContext) => 
            { 
                // Convert dto to entity 
                Event evt = newEvent.ToEntity();
                // Add event
                dbContext.Events.Add(evt);
                // Commit changes
                await dbContext.SaveChangesAsync();
                return Results.CreatedAtRoute(getEventEndpoint, new { id = evt.Id }, evt.ToEventDetailsDto());
            });
            
            // Update events by id 
            group.MapPut("/{id}", async(int id, UpdateEventDto updatedEvent, EventDbContext dbContext) =>{
            var existingEvent = await dbContext.Events.FindAsync(id);

            if (existingEvent is null){
                return Results.NotFound();
            }
            dbContext.Entry(existingEvent)
            .CurrentValues
            .SetValues(updatedEvent.ToEntity(id));
            await dbContext.SaveChangesAsync();
            return Results.NoContent();
            });


            // Delete events by id
            group.MapDelete("/{id}", async(int id, EventDbContext dbContext) =>
            {
                await dbContext.Events.Where(evt => evt.Id == id).ExecuteDeleteAsync();
                return Results.NoContent();
            });

            // Delete events by name
            group.MapDelete("/event_name/{name}", async (string name, EventDbContext dbContext) =>
            {
                // Search for event by name
                var evt = await dbContext.Events.FirstOrDefaultAsync(e => e.Name == name);
                if (evt == null)
                {
                    return Results.NotFound();
                }

                // Delete the event
                dbContext.Events.Remove(evt);
                await dbContext.SaveChangesAsync();

                return Results.NoContent();
            });

            return group;
        }
    }