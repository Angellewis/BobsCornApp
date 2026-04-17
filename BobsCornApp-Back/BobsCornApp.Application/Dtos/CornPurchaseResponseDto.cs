namespace BobsCornApp.Application.Dtos;

public class CornPurchaseResponseDto
{
    public string Corn { get; init; } = "\uD83C\uDF3D";

    public string Message { get; init; } = string.Empty;

    public DateTimeOffset PurchasedAtUtc { get; init; }
}
