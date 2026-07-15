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
    public async Task<ActionResult<Preference?>> GetPreference(CancellationToken cancellationToken) {
        string? userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userIdString is null) {
            return Unauthorized();
        }

        Preference? preference = await _preferenceRepository.GetPreference(Guid.Parse(userIdString),
                cancellationToken);

        if (preference is null) {
            return NotFound();
        }

        return Ok(preference);
    }

    [HttpGet("matching-users")]
    public async Task<ActionResult<List<Guid>>> GetMatchingUserIds([FromQuery] EventCity city,
            [FromQuery] EventCategory category, CancellationToken cancellationToken) {
        return Ok(await _preferenceRepository.GetMatchingUserIds(city, category, cancellationToken));
    }

    [Authorize]
    [HttpPatch]
    public async Task<IActionResult>
            UpdatePreference(UpdatePreferenceDto request, CancellationToken cancellationToken) {
        string? userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userIdString is null) {
            return Unauthorized();
        }

        Preference? preference = await _preferenceRepository.GetPreference(Guid.Parse(userIdString),
                cancellationToken);

        if (preference is not null) {
            if (request.City is not null) {
                preference.City = request.City;
            }

            if (request.Category is not null) {
                preference.Category = request.Category;
            }

            await _preferenceRepository.UpdatePreference(preference, cancellationToken);
        } else {
            Preference newPreference = new() {
                    UserId = Guid.Parse(userIdString),
                    City = request.City,
                    Category = request.Category
            };

            await _preferenceRepository.CreatePreference(newPreference, cancellationToken);
        }

        return NoContent();
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> DeletePreference(CancellationToken cancellationToken) {
        string? userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userIdString is null) {
            return Unauthorized();
        }

        await _preferenceRepository.DeletePreference(Guid.Parse(userIdString), cancellationToken);

        return NoContent();
    }
}