using PreferenceService.Application;
using PreferenceService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using PreferenceService.Application.DTOs;

namespace PreferenceService.API.Controllers;

[ApiController]
[Route("api/preferences")]
public class PreferenceController : ControllerBase {
    
    readonly IPreferenceRepository _preferenceRepository;
    
    public PreferenceController(IPreferenceRepository preferenceRepository) {
        _preferenceRepository = preferenceRepository;
    }
    
    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<Preference>> GetPreference(Guid userId) {
        // TODO: proper return types
        return Ok(await _preferenceRepository.GetPreference(userId));
    }
    
    [HttpPatch("{userId:guid}")]
    public async Task<IActionResult> UpdatePreference(Guid userId, UpdatePreferenceDto request) {
        Preference? preference = await _preferenceRepository.GetSpecificPreference(userId);

        if (preference is not null) {
            // TODO: implement patching data

            await _preferenceRepository.UpdatePreference(preference);
        }
        else {
            return NotFound();
        }
        
        // TODO: proper return types
        return Ok();
    }
    
}