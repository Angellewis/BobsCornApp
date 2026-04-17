namespace BobsCornApp.Application.Options;

public class CornRateLimitOptions
{
    public const string SectionName = "CornRateLimit";

    public int WindowSeconds { get; init; } = 60;
}
