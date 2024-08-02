using BookingService.API.Dto.booking;
using BookingService.API.Entities;
namespace BookingService.API.Mapping;
public static class BookingMapping
{
    // Maps a create booking data transfer object to a database entity 
    public static Booking ToEntity(this CreateBookingDto booking){
        return new Booking(){
            Username = booking.Username,
            EventId = booking.EventId,
            Amount = booking.Amount
        };
    }

    // Maps a update booking data transfer object to a database entity 
    public static Booking ToEntity(this UpdateBookingDto booking, int id){
        return new Booking(){
            Id = id,
            Username = booking.Username,
            EventId = booking.EventId,
            Amount = booking.Amount
        };
    }

    //Maps an entity to a summarized booking data transfer object
    public static BookingSummaryDto ToBookingSummaryDto(this Booking booking){
        return new(
            booking.Username,
            booking.EventId
        );
    }

     //Maps an entity to a detailed booking data transfer object
    public static BookingDetailsDto ToBookingDetailsDto(this Booking booking){
        return new BookingDetailsDto(){
            Id = booking.Id,
            Username = booking.Username, 
            EventId = booking.EventId,
            EventName = "",
            Amount = booking.Amount
        };
    }
}