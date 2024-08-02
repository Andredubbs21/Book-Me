using BookingService.API.Entities;
using Microsoft.EntityFrameworkCore;
namespace BookingService.API.Data;

public class BookingDbContext(DbContextOptions<BookingDbContext> options) : DbContext(options)
{
    public DbSet<Booking> Bookings => Set<Booking>();

}