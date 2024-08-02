using EventService.API.Entities;
using Microsoft.EntityFrameworkCore;
namespace EventService.API.Data;


public class EventDbContext(DbContextOptions<EventDbContext> options) : DbContext(options)
{

    public DbSet<Event> Events => Set<Event>();

}