namespace BobsCornApp.Application.Dtos;

public class RateLimitExceededResponseDto : BaseResponseDto
{
    public int RetryAfterSeconds { get; init; }
}
