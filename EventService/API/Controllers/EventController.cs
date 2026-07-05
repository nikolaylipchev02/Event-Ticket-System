using EventService.Application;
using EventService.Application.DTOs;
using EventService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EventService.API.Controllers;

[ApiController]
[Route("api/events")]
public class EventController : ControllerBase {
    
    readonly IEventsRepository _eventsRepository;
    
    public EventController(IEventsRepository eventsRepository) {
        _eventsRepository = eventsRepository;
    }
    
    // Get Events
    [HttpGet]
    public async Task<ActionResult<List<Event>>> GetEvents() {
        // TODO: proper return types
        return Ok(await _eventsRepository.GetEvents());
    }
    
    // Get Specific Event
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Event>> GetSpecificEvent(Guid id) {
        Event? existingEvent = await _eventsRepository.GetSpecificEvent(id);

        if (existingEvent is null) {
            return NotFound();
        }
        
        // TODO: proper return types
        return Ok(existingEvent);
    }

    // Create Event
    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto request) {
        Event eventToCreate = new() {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
        };

        await _eventsRepository.CreateEvent(eventToCreate);

        // TODO: proper return types
        return Ok();
    }

    // Update Event
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] UpdateEventDto request) {
        Event? existingEvent = await _eventsRepository.GetSpecificEvent(id);

        if (existingEvent is null) {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(request.Title)) {
            existingEvent.Title = request.Title;
        }
        
        if (!string.IsNullOrEmpty(request.Description)) {
            existingEvent.Description = request.Description;
        }

        await _eventsRepository.UpdateEvent(existingEvent);
        
        // TODO: proper return types
        return Ok();
    }
    
    // Delete Event
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteEvent(Guid id) {
        await _eventsRepository.DeleteEvent(id);
        
        // TODO: proper return types
        return Ok();
    }
}