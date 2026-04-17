using BobsCornApp.Application.Interfaces;
using BobsCornApp.Application.Options;
using BobsCornApp.Domain.Entities;
using Microsoft.Extensions.Options;

namespace BobsCornApp.Application.Services;

public class CornPurchaseService : ICornPurchaseService
{
    private readonly ICornPurchaseRepository _cornPurchaseRepository;
    private readonly CornRateLimitOptions _rateLimitOptions;
    private readonly TimeProvider _timeProvider;
    private readonly SemaphoreSlim _purchaseLock = new(1, 1);

    public CornPurchaseService(
        ICornPurchaseRepository cornPurchaseRepository,
        IOptions<CornRateLimitOptions> rateLimitOptions,
        TimeProvider timeProvider)
    {
        _cornPurchaseRepository = cornPurchaseRepository;
        _rateLimitOptions = rateLimitOptions.Value;
        _timeProvider = timeProvider;
    }

    public async Task<CornPurchaseResult> TryBuyCornAsync(string clientId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);

        var now = _timeProvider.GetUtcNow();
        var limitWindow = TimeSpan.FromSeconds(Math.Max(1, _rateLimitOptions.WindowSeconds));

        await _purchaseLock.WaitAsync(cancellationToken);
        try
        {
            var lastPurchase = await _cornPurchaseRepository.GetLastPurchaseAsync(clientId, cancellationToken);
            if (lastPurchase is not null)
            {
                var elapsed = now - lastPurchase.PurchasedAtUtc;
                if (elapsed < limitWindow)
                {
                    return CornPurchaseResult.TooManyRequests(limitWindow - elapsed);
                }
            }

            var purchase = new CornPurchase
            {
                ClientId = clientId.Trim(),
                PurchasedAtUtc = now
            };

            await _cornPurchaseRepository.SavePurchaseAsync(purchase, cancellationToken);

            return CornPurchaseResult.Success(purchase);
        }
        finally
        {
            _purchaseLock.Release();
        }
    }
}
