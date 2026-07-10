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
    readonly IBookingService _bookingService;

    public BookingController(IBookingRepository bookingRepository, IBookingService bookingService) {
        _bookingRepository = bookingRepository;
        _bookingService = bookingService;
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

        try {
            await _bookingService.Book(Guid.Parse(userIdString), request.EventId);
            return Ok();
        } catch (KeyNotFoundException) {
            return NotFound();
        } catch (InvalidOperationException) {
            return Conflict("No tickets available");
        }
    }

    [Authorize]
    [HttpDelete("{bookingId:guid}")]
    public async Task<IActionResult> CancelBooking(Guid bookingId) {
        string? userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userIdString is null) {
            return Forbid();
        }

        try {
            await _bookingRepository.CancelBooking(Guid.Parse(userIdString), bookingId);
            return Ok();
        } catch (KeyNotFoundException) {
            return NotFound();
        } catch (InvalidOperationException) {
            return Conflict("Booking is already cancelled");
        }
    }
}
