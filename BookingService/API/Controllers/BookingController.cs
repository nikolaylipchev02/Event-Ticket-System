using BookingService.Application;
using BookingService.Application.DTOs;
using BookingService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.API.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingController :ControllerBase {

    readonly IBookingRepository _bookingRepository;

    public BookingController(IBookingRepository bookingRepository) {
        _bookingRepository = bookingRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<Booking>>> GetBookings([FromQuery] Guid userId) {
        // TODO: proper return types
        return Ok(await _bookingRepository.GetBookings(userId));
    }
    
    [HttpPost]
    public async Task<IActionResult> Book([FromBody] CreateBookingDto request) {
        Booking booking = new Booking() {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            EventId = request.EventId,
            Status = BookingStatus.Booked,
            BookedAt = DateTime.UtcNow
        };
        
        await _bookingRepository.Book(booking);

        // TODO: proper return types
        return Ok();
    }
    
    [HttpDelete("{id:userId}")]
    public async Task<ActionResult<List<Booking>>> CancelBooking(Guid userId) {
        await _bookingRepository.CancelBooking(userId);
                
        // TODO: proper return types
        return Ok();
    }
}