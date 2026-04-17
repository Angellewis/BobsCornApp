namespace BobsCornApp.Application.Interfaces;

public interface ICornPurchaseService
{
    Task<CornPurchaseResult> TryBuyCornAsync(string clientId, CancellationToken cancellationToken = default);
}
