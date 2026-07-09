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
    public async Task<ActionResult<List<Event>>> GetEvents() {
        // TODO: proper return types
        return Ok(await _eventRepository.GetEvents());
    }

    // TODO: think about a better solution
    [HttpGet("filtered")]
    public async Task<ActionResult<List<Event>>> GetFilteredEvents([FromQuery] FilterEventDto filter) {
        if (filter.MinPrice is not null && filter.MaxPrice is not null && filter.MinPrice > filter.MaxPrice) {
            return BadRequest("Min Price cannot be greater than Max Price");
        }

        if (filter.FromDate is not null && filter.ToDate is not null && filter.FromDate > filter.ToDate) {
            return BadRequest("From Date cannot be greater than To Date");
        }

        // TODO: proper return types
        return Ok(await _eventRepository.GetFilteredEvents(filter));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Event>> GetSpecificEvent(Guid id) {
        Event? existingEvent = await _eventRepository.GetSpecificEvent(id);

        if (existingEvent is null) {
            return NotFound();
        }

        // TODO: proper return types
        return Ok(existingEvent);
    }

    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto request) {
        // TODO: validate business rules
        Event eventToCreate = new() {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                City = request.City,
                Category = request.Category,
                Price = request.Price,
                Date = request.Date,
                CreatedAt = DateTime.UtcNow
        };

        await _eventRepository.CreateEvent(eventToCreate);

        // TODO: proper return types
        return Ok();
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] UpdateEventDto request) {
        Event? existingEvent = await _eventRepository.GetSpecificEvent(id);

        if (existingEvent is null) {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(request.Title)) {
            existingEvent.Title = request.Title;
        }

        if (!string.IsNullOrEmpty(request.Description)) {
            existingEvent.Description = request.Description;
        }

        await _eventRepository.UpdateEvent(existingEvent);

        // TODO: proper return types
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteEvent(Guid id) {
        await _eventRepository.DeleteEvent(id);

        // TODO: proper return types
        return Ok();
    }
}