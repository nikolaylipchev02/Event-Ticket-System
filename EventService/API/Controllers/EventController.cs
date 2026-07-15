using EventService.Application;
using EventService.Application.DTOs;
using EventService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EventService.API.Controllers;

[ApiController]
[Route("api/events")]
public class EventController : ControllerBase {
    readonly IEventRepository _eventRepository;

    public EventController(IEventRepository eventRepository) {
        _eventRepository = eventRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<Event>>> GetEvents(CancellationToken cancellationToken) {
        return Ok(await _eventRepository.GetEvents(cancellationToken));
    }

    // TODO: think about a better solution
    [HttpGet("filtered")]
    public async Task<ActionResult<List<Event>>> GetFilteredEvents([FromQuery] FilterEventDto filter,
            CancellationToken cancellationToken) {
        if (filter.MinPrice is not null && filter.MaxPrice is not null && filter.MinPrice > filter.MaxPrice) {
            return BadRequest("Min Price cannot be greater than Max Price");
        }

        if (filter.FromDate is not null && filter.ToDate is not null && filter.FromDate > filter.ToDate) {
            return BadRequest("From Date cannot be greater than To Date");
        }

        return Ok(await _eventRepository.GetFilteredEvents(filter, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Event>> GetSpecificEvent(Guid id, CancellationToken cancellationToken) {
        Event? existingEvent = await _eventRepository.GetSpecificEvent(id, cancellationToken);

        if (existingEvent is null) {
            return NotFound();
        }

        return Ok(existingEvent);
    }

    [HttpPost]
    public async Task<IActionResult>
            CreateEvent([FromBody] CreateEventDto request, CancellationToken cancellationToken) {
        // TODO: validate business rules
        Event eventToCreate = new() {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                City = request.City,
                Category = request.Category,
                Price = request.Price,
                Date = NormalizeEventDate(request.Date),
                CreatedAt = DateTime.UtcNow,
                TotalTickets = request.TotalTickets
        };

        await _eventRepository.CreateEvent(eventToCreate, cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] UpdateEventDto request,
            CancellationToken cancellationToken) {
        Event? existingEvent = await _eventRepository.GetSpecificEvent(id, cancellationToken);

        if (existingEvent is null) {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(request.Title)) {
            existingEvent.Title = request.Title;
        }

        if (!string.IsNullOrEmpty(request.Description)) {
            existingEvent.Description = request.Description;
        }

        if (request.City is not null) {
            existingEvent.City = request.City.Value;
        }

        if (request.Category is not null) {
            existingEvent.Category = request.Category.Value;
        }

        if (request.Price is not null) {
            existingEvent.Price = request.Price.Value;
        }

        if (request.Date is not null) {
            existingEvent.Date = NormalizeEventDate(request.Date.Value);
        }

        await _eventRepository.UpdateEvent(existingEvent, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteEvent(Guid id, CancellationToken cancellationToken) {
        await _eventRepository.DeleteEvent(id, cancellationToken);

        return NoContent();
    }

    static DateTime NormalizeEventDate(DateTime date) {
        return date.Kind switch {
                DateTimeKind.Utc => date,
                DateTimeKind.Local => date.ToUniversalTime(),
                _ => DateTime.SpecifyKind(date, DateTimeKind.Local).ToUniversalTime()
        };
    }
}