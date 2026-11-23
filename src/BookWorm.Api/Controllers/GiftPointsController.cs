using System.Security.Claims;
using BookWorm.Api.Models;
using BookWorm.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookWorm.Api.Controllers;

[ApiController]
[Route("api/me/gift-points")]
[Authorize]
public class GiftPointsController : ControllerBase
{
    private readonly IGiftPointService _giftPointService;

    public GiftPointsController(IGiftPointService giftPointService)
    {
        _giftPointService = giftPointService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(GiftPointsSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GiftPointsSummaryDto>> GetGiftPoints()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var summary = await _giftPointService.GetUserGiftPointsAsync(userId);
        return Ok(summary);
    }
}
