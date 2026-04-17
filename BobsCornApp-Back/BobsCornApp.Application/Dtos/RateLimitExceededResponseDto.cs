namespace BobsCornApp.Application.Dtos;

public class RateLimitExceededResponseDto
{
    public string Message { get; init; } = string.Empty;

    public int RetryAfterSeconds { get; init; }
}
