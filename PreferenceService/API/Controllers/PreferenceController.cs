using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SharedContracts;
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

    [HttpGet("matching-users")]
    public async Task<ActionResult<List<Guid>>> GetMatchingUserIds([FromQuery] EventCity city,
            [FromQuery] EventCategory category) {
        return Ok(await _preferenceRepository.GetMatchingUserIds(city, category));
    }

    [Authorize]
    [HttpPatch]
    public async Task<IActionResult> UpdatePreference(UpdatePreferenceDto request) {
        string? userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userIdString is null) {
            return Forbid();
        }

        Preference? preference = await _preferenceRepository.GetPreference(Guid.Parse(userIdString));

        if (preference is not null) {
            if (request.City is not null) {
                preference.City = request.City;
            }

            if (request.Category is not null) {
                preference.Category = request.Category;
            }

            await _preferenceRepository.UpdatePreference(preference);
        } else {
            Preference newPreference = new() {
                    UserId = Guid.Parse(userIdString),
                    City = request.City,
                    Category = request.Category
            };

            await _preferenceRepository.CreatePreference(newPreference);
        }

        // TODO: proper return types
        return Ok();
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> DeletePreference() {
        string? userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userIdString is null) {
            return Forbid();
        }

        await _preferenceRepository.DeletePreference(Guid.Parse(userIdString));

        // TODO: proper return types
        return Ok();
    }
}