using BobsCornApp.Domain.Entities;

namespace BobsCornApp.Application.Interfaces;

public interface ICornPurchaseRepository
{
    Task<CornPurchase?> GetLastPurchaseAsync(string clientId, CancellationToken cancellationToken = default);

    Task SavePurchaseAsync(CornPurchase purchase, CancellationToken cancellationToken = default);
}
