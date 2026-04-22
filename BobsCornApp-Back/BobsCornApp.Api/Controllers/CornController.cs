using AutoMapper;
using BobsCornApp.Application.Dtos;
using BobsCornApp.Application.Interfaces;
using BobsCornApp.Application.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BobsCornApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CornController : ControllerBase
{
    private readonly ICornPurchaseService _cornPurchaseService;
    private readonly IMapper _mapper;
    private readonly CornRateLimitOptions _rateLimitOptions;

    public CornController(
        ICornPurchaseService cornPurchaseService,
        IMapper mapper,
        IOptions<CornRateLimitOptions> rateLimitOptions)
    {
        _cornPurchaseService = cornPurchaseService;
        _mapper = mapper;
        _rateLimitOptions = rateLimitOptions.Value;
    }

    [HttpPost("buy")]
    [ProducesResponseType(typeof(CornPurchaseResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RateLimitExceededResponseDto), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> BuyCorn(
        [FromQuery] string? clientId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(clientId))
        {
            return BadRequest(new BaseResponseDto
            {
                Message = "The clientId query parameter is required."
            });
        }

        var normalizedClientId = clientId.Trim();
        var result = await _cornPurchaseService.TryBuyCornAsync(normalizedClientId, cancellationToken);

        if (!result.IsSuccessful)
        {
            var retryAfterSeconds = Math.Max(1, (int)Math.Ceiling(result.RetryAfter!.Value.TotalSeconds));
            Response.Headers.RetryAfter = retryAfterSeconds.ToString();

            return StatusCode(
                StatusCodes.Status429TooManyRequests,
                new RateLimitExceededResponseDto
                {
                    Message = BuildRateLimitExceededMessage(),
                    RetryAfterSeconds = retryAfterSeconds
                });
        }

        return Ok(_mapper.Map<CornPurchaseResponseDto>(result.Purchase));
    }

    private string BuildRateLimitExceededMessage()
    {
        var windowSeconds = Math.Max(1, _rateLimitOptions.WindowSeconds);
        return $"Rate limit exceeded. Clients can buy at most 1 corn every {windowSeconds} second{(windowSeconds == 1 ? string.Empty : "s")}.";
    }
}
