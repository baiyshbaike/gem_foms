using Application.Authorization;
using Application.MedCards;
using Application.Tenants;
using Contracts.MedCards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/med-cards")]
[Authorize]
public sealed class MedCardsController : ControllerBase
{
    private readonly IMedCardService _medCardService;
    private readonly ITenantContext _tenant;

    public MedCardsController(IMedCardService medCardService, ITenantContext tenant)
    {
        _medCardService = medCardService;
        _tenant = tenant;
    }

    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.MedCardRead)]
    public async Task<ActionResult<IReadOnlyList<MedCardDto>>> Get(
        [FromQuery] List<string>? tenantIds,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var medCards = await _medCardService.GetAsync(userId.Value, tenantIds, cancellationToken);
        return medCards is null ? Forbid() : Ok(medCards);
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "Permission:" + Permissions.MedCardRead)]
    public async Task<ActionResult<MedCardDto>> GetById(long id, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var medCard = await _medCardService.GetByIdAsync(userId.Value, _tenant.RequiredTenantId, id, cancellationToken);
        return medCard is null ? NotFound() : Ok(medCard);
    }

    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.MedCardCreate)]
    public async Task<ActionResult<MedCardDto>> Create(CreateMedCardRequest request, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _medCardService.CreateAsync(
            _tenant.RequiredTenantId,
            userId.Value,
            request,
            cancellationToken);

        return result.Status switch
        {
            MedCardCommandStatus.Succeeded => CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value),
            MedCardCommandStatus.Forbidden => Forbid(),
            MedCardCommandStatus.NotFound => NotFound(),
            MedCardCommandStatus.Conflict => Conflict(),
            _ => Conflict()
        };
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "Permission:" + Permissions.MedCardUpdate)]
    public async Task<ActionResult<MedCardDto>> Update(
        long id,
        UpdateMedCardRequest request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _medCardService.UpdateAsync(
            _tenant.RequiredTenantId,
            id,
            userId.Value,
            request,
            cancellationToken);

        return result.Status switch
        {
            MedCardCommandStatus.Succeeded => Ok(result.Value),
            MedCardCommandStatus.Forbidden => Forbid(),
            MedCardCommandStatus.NotFound => NotFound(),
            MedCardCommandStatus.Conflict => Conflict(),
            _ => Conflict()
        };
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "Permission:" + Permissions.MedCardDelete)]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _medCardService.DeleteAsync(
            _tenant.RequiredTenantId,
            id,
            userId.Value,
            cancellationToken);

        return result.Status switch
        {
            MedCardCommandStatus.Succeeded => NoContent(),
            MedCardCommandStatus.Forbidden => Forbid(),
            MedCardCommandStatus.NotFound => NotFound(),
            MedCardCommandStatus.Conflict => Conflict(),
            _ => Conflict()
        };
    }

    private long? CurrentUserId()
    {
        var sub = User.FindFirst("sub")?.Value;
        return long.TryParse(sub, out var userId) ? userId : null;
    }
}