using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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
    
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<Preference?>> GetPreference() {
        string? userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userIdString is null) {
            return Forbid();
        }
        
        Preference? preference = await _preferenceRepository.GetPreference(Guid.Parse(userIdString));

        if (preference is null) {
            return NotFound();
        }
        
        // TODO: proper return types
        return Ok(preference);
    }
    
    [Authorize]
    [HttpPatch]
    public async Task<IActionResult> UpdatePreference(UpdatePreferenceDto request) {
        string? userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userIdString is null) {
            return Forbid();
        }
        
        Preference? preference = await _preferenceRepository.GetSpecificPreference(Guid.Parse(userIdString));

        if (preference is not null) {
            // TODO: implement patching data
            await _preferenceRepository.UpdatePreference(preference);
        } else {
            return NotFound();
        }
        
        // TODO: proper return types
        return Ok();
    }
}
