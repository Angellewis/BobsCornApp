using AutoMapper;
using BobsCornApp.Application.Dtos;
using BobsCornApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BobsCornApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CornController : ControllerBase
{
    private readonly ICornPurchaseService _cornPurchaseService;
    private readonly IMapper _mapper;

    public CornController(ICornPurchaseService cornPurchaseService, IMapper mapper)
    {
        _cornPurchaseService = cornPurchaseService;
        _mapper = mapper;
    }

    [HttpPost("buy")]
    [ProducesResponseType(typeof(CornPurchaseResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RateLimitExceededResponseDto), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> BuyCorn(
        [FromHeader(Name = "X-Client-Id")] string? clientId,
        CancellationToken cancellationToken)
    {
        var effectiveClientId = ResolveClientId(clientId);
        var result = await _cornPurchaseService.TryBuyCornAsync(effectiveClientId, cancellationToken);

        if (!result.IsSuccessful)
        {
            var retryAfterSeconds = Math.Max(1, (int)Math.Ceiling(result.RetryAfter!.Value.TotalSeconds));
            Response.Headers.RetryAfter = retryAfterSeconds.ToString();

            return StatusCode(
                StatusCodes.Status429TooManyRequests,
                new RateLimitExceededResponseDto
                {
                    Message = "Rate limit exceeded. Clients can buy at most 1 corn per minute.",
                    RetryAfterSeconds = retryAfterSeconds
                });
        }

        return Ok(_mapper.Map<CornPurchaseResponseDto>(result.Purchase));
    }

    private string ResolveClientId(string? clientId)
    {
        if (!string.IsNullOrWhiteSpace(clientId))
        {
            return clientId.Trim();
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
    }
}
