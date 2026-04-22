namespace BobsCornApp.Application.Dtos;

public class CornPurchaseResponseDto : BaseResponseDto
{
    public string Corn { get; init; } = "\uD83C\uDF3D";

    public DateTimeOffset PurchasedAtUtc { get; init; }
}
