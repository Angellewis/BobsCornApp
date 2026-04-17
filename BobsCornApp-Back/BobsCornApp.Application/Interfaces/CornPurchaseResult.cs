using BobsCornApp.Domain.Entities;

namespace BobsCornApp.Application.Interfaces;

public class CornPurchaseResult
{
    private CornPurchaseResult(bool isSuccessful, CornPurchase? purchase, TimeSpan? retryAfter)
    {
        IsSuccessful = isSuccessful;
        Purchase = purchase;
        RetryAfter = retryAfter;
    }

    public bool IsSuccessful { get; }

    public CornPurchase? Purchase { get; }

    public TimeSpan? RetryAfter { get; }

    public static CornPurchaseResult Success(CornPurchase purchase) => new CornPurchaseResult(true, purchase, null);

    public static CornPurchaseResult TooManyRequests(TimeSpan retryAfter) => new CornPurchaseResult(false, null, retryAfter);
}
