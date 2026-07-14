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
            return Unauthorized();
        }

        return Ok(await _bookingRepository.GetBookings(Guid.Parse(userIdString)));
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Book(
            [FromBody] CreateBookingDto request,
            [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey
    ) {
        string? userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userIdString is null) {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(idempotencyKey)) {
            return BadRequest("Idempotency-Key header is required");
        }

        try {
            await _bookingService.Book(Guid.Parse(userIdString), request.EventId, idempotencyKey);
            return NoContent();
        } catch (KeyNotFoundException) {
            return NotFound();
        } catch (InvalidOperationException e) {
            return Conflict(e.Message);
        }
    }

    [Authorize]
    [HttpDelete("{bookingId:guid}")]
    public async Task<IActionResult> CancelBooking(Guid bookingId) {
        string? userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userIdString is null) {
            return Unauthorized();
        }

        try {
            await _bookingRepository.CancelBooking(Guid.Parse(userIdString), bookingId);
            return NoContent();
        } catch (KeyNotFoundException) {
            return NotFound();
        } catch (InvalidOperationException) {
            return Conflict("Booking is already cancelled");
        }
    }
}