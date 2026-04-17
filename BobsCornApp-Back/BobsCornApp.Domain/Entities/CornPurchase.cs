namespace BobsCornApp.Domain.Entities;

public class CornPurchase
{
    public required string ClientId { get; init; }

    public DateTimeOffset PurchasedAtUtc { get; init; }
}
