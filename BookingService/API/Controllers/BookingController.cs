using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BookingService.Application;
using BookingService.Application.DTOs;
using BookingService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.API.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingController : ControllerBase {
    readonly IBookingRepository _bookingRepository;

    public BookingController(IBookingRepository bookingRepository) {
        _bookingRepository = bookingRepository;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<Booking>>> GetBookings() {
        string? userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userIdString is null) {
            return Forbid();
        }

        // TODO: proper return types
        return Ok(await _bookingRepository.GetBookings(Guid.Parse(userIdString)));
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Book([FromBody] CreateBookingDto request) {
        string? userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userIdString is null) {
            return Forbid();
        }

        Booking booking = new() {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse(userIdString),
                EventId = request.EventId,
                Status = BookingStatus.Booked,
                BookedAt = DateTime.UtcNow
        };

        await _bookingRepository.Book(booking);

        // TODO: proper return types
        return Ok();
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> CancelBooking(Guid id) {
        string? userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userIdString is null) {
            return Forbid();
        }

        await _bookingRepository.CancelBooking(id);

        // TODO: proper return types
        return Ok();
    }
}