using BobsCornApp.Application.Interfaces;
using BobsCornApp.Application.Options;
using BobsCornApp.Application.Services;
using BobsCornApp.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BobsCornApp.UnitTests.Application;

[TestClass]
public class CornPurchaseServiceTests
{
    [TestMethod]
    public async Task TryBuyCornShouldSavePurchaseAndReturnSuccessWhenClientIsUnderLimit()
    {
        var now = new DateTimeOffset(2026, 4, 16, 12, 0, 0, TimeSpan.Zero);
        var repository = new Mock<ICornPurchaseRepository>();
        repository.Setup(repo => repo.GetLastPurchaseAsync("  client-1  ", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CornPurchase?)null);
        repository.Setup(repo => repo.SavePurchaseAsync(It.IsAny<CornPurchase>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateService(repository.Object, now, 60);
        var result = await service.TryBuyCornAsync("  client-1  ");

        Assert.IsTrue(
            result.IsSuccessful
            && result.Purchase is not null
            && result.Purchase.ClientId == "client-1"
            && result.Purchase.PurchasedAtUtc == now,
            "Expected a successful purchase with a trimmed client id and the current timestamp.");
    }

    [TestMethod]
    public async Task TryBuyCornShouldReturnTooManyRequestsWhenPurchaseIsInsideWindow()
    {
        var now = new DateTimeOffset(2026, 4, 16, 12, 0, 30, TimeSpan.Zero);
        var repository = new Mock<ICornPurchaseRepository>();
        repository.Setup(repo => repo.GetLastPurchaseAsync("client-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CornPurchase
            {
                ClientId = "client-1",
                PurchasedAtUtc = now.AddSeconds(-10)
            });

        var service = CreateService(repository.Object, now, 60);
        var result = await service.TryBuyCornAsync("client-1");

        Assert.IsTrue(
            !result.IsSuccessful
            && result.Purchase is null
            && result.RetryAfter == TimeSpan.FromSeconds(50),
            "Expected the purchase to be rejected with a 50 second retry window.");
    }

    [TestMethod]
    public async Task TryBuyCornShouldAllowPurchaseWhenPreviousPurchaseIsOutsideWindow()
    {
        var now = new DateTimeOffset(2026, 4, 16, 12, 1, 0, TimeSpan.Zero);
        var repository = new Mock<ICornPurchaseRepository>();
        repository.Setup(repo => repo.GetLastPurchaseAsync("client-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CornPurchase
            {
                ClientId = "client-1",
                PurchasedAtUtc = now.AddSeconds(-60)
            });
        repository.Setup(repo => repo.SavePurchaseAsync(It.IsAny<CornPurchase>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateService(repository.Object, now, 60);
        var result = await service.TryBuyCornAsync("client-1");

        Assert.IsTrue(
            result.IsSuccessful
            && result.Purchase is not null
            && result.Purchase.PurchasedAtUtc == now,
            "Expected a new purchase once the full window has elapsed.");
    }

    [TestMethod]
    public async Task TryBuyCornShouldUseMinimumOneSecondWindowWhenConfiguredWindowIsZero()
    {
        var now = new DateTimeOffset(2026, 4, 16, 12, 0, 0, TimeSpan.Zero);
        var repository = new Mock<ICornPurchaseRepository>();
        repository.Setup(repo => repo.GetLastPurchaseAsync("client-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CornPurchase
            {
                ClientId = "client-1",
                PurchasedAtUtc = now.AddMilliseconds(-500)
            });

        var service = CreateService(repository.Object, now, 0);
        var result = await service.TryBuyCornAsync("client-1");

        Assert.IsTrue(
            !result.IsSuccessful
            && result.RetryAfter.HasValue
            && result.RetryAfter.Value.TotalMilliseconds >= 499
            && result.RetryAfter.Value.TotalMilliseconds <= 501,
            "Expected a zero-second configuration to behave like a one-second window.");
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("\t")]
    public async Task TryBuyCornShouldThrowArgumentExceptionWhenClientIdIsBlank(string clientId)
    {
        var repository = new Mock<ICornPurchaseRepository>();
        var service = CreateService(repository.Object, DateTimeOffset.UtcNow, 60);

        await Assert.ThrowsExceptionAsync<ArgumentException>(() => service.TryBuyCornAsync(clientId));
    }

    private static CornPurchaseService CreateService(
        ICornPurchaseRepository repository,
        DateTimeOffset now,
        int windowSeconds)
    {
        return new CornPurchaseService(
            repository,
            Options.Create(new CornRateLimitOptions { WindowSeconds = windowSeconds }),
            new FakeTimeProvider(now));
    }

    private sealed class FakeTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
