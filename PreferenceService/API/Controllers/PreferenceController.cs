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
    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<Preference>> GetPreference(Guid userId) {
        string? userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userIdString is null) {
            return Forbid();
        }
        
        Guid userIdGuid = Guid.Parse(userIdString);
        
        if (userId != userIdGuid) {
            return Forbid();
        }
        
        // TODO: proper return types
        return Ok(await _preferenceRepository.GetPreference(userIdGuid));
    }
    
    [Authorize]
    [HttpPatch("{userId:guid}")]
    public async Task<IActionResult> UpdatePreference(Guid userId, UpdatePreferenceDto request) {
        string? userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userIdString is null) {
            return Forbid();
        }
        
        Guid userIdGuid = Guid.Parse(userIdString);
        
        if (userId != userIdGuid) {
            return Forbid();
        }

        
        Preference? preference = await _preferenceRepository.GetSpecificPreference(userIdGuid);

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