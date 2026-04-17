using AutoMapper;
using BobsCornApp.Api.Controllers;
using BobsCornApp.Application.Dtos;
using BobsCornApp.Application.Interfaces;
using BobsCornApp.Application.Mapping;
using BobsCornApp.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Net;

namespace BobsCornApp.UnitTests.Api;

[TestClass]
public class CornControllerTests
{
    [TestMethod]
    public async Task BuyCornShouldReturnOkResponseWhenSuccessful()
    {
        var purchase = new CornPurchase
        {
            ClientId = "client-1",
            PurchasedAtUtc = new DateTimeOffset(2026, 4, 16, 12, 0, 0, TimeSpan.Zero)
        };
        var service = new Mock<ICornPurchaseService>();
        service.Setup(s => s.TryBuyCornAsync("client-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CornPurchaseResult.Success(purchase));
        var controller = CreateController(service.Object);

        var result = await controller.BuyCorn("  client-1  ", CancellationToken.None);

        Assert.IsTrue(
            result is OkObjectResult { Value: CornPurchaseResponseDto dto }
            && dto.Message == "Corn purchased successfully."
            && dto.Corn == "\uD83C\uDF3D"
            && dto.PurchasedAtUtc == purchase.PurchasedAtUtc,
            "Expected an OK response with the mapped purchase payload.");
    }

    [TestMethod]
    public async Task BuyCornShouldReturnTooManyRequestsWhenRateLimited()
    {
        var service = new Mock<ICornPurchaseService>();
        service.Setup(s => s.TryBuyCornAsync("client-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CornPurchaseResult.TooManyRequests(TimeSpan.FromSeconds(7.2)));
        var controller = CreateController(service.Object);

        var result = await controller.BuyCorn("client-1", CancellationToken.None);

        Assert.IsTrue(
            result is ObjectResult
            {
                StatusCode: StatusCodes.Status429TooManyRequests,
                Value: RateLimitExceededResponseDto dto
            }
            && dto.RetryAfterSeconds == 8
            && controller.Response.Headers.RetryAfter == "8",
            "Expected a 429 response with a rounded retry-after header and payload.");
    }

    [TestMethod]
    public async Task BuyCornShouldUseRemoteIpWhenHeaderIsMissing()
    {
        var service = new Mock<ICornPurchaseService>();
        service.Setup(s => s.TryBuyCornAsync("10.0.0.7", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CornPurchaseResult.Success(new CornPurchase
            {
                ClientId = "10.0.0.7",
                PurchasedAtUtc = DateTimeOffset.UtcNow
            }));
        var controller = CreateController(service.Object, IPAddress.Parse("10.0.0.7"));

        await controller.BuyCorn(null, CancellationToken.None);

        Assert.IsTrue(
            service.Invocations.Count == 1
            && service.Invocations[0].Arguments[0] is string clientId
            && clientId == "10.0.0.7",
            "Expected the controller to fall back to the remote IP address.");
    }

    [TestMethod]
    public async Task BuyCornShouldUseAnonymousWhenHeaderAndRemoteIpAreMissing()
    {
        var service = new Mock<ICornPurchaseService>();
        service.Setup(s => s.TryBuyCornAsync("anonymous", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CornPurchaseResult.Success(new CornPurchase
            {
                ClientId = "anonymous",
                PurchasedAtUtc = DateTimeOffset.UtcNow
            }));
        var controller = CreateController(service.Object);

        await controller.BuyCorn(null, CancellationToken.None);

        Assert.IsTrue(
            service.Invocations.Count == 1
            && service.Invocations[0].Arguments[0] is string clientId
            && clientId == "anonymous",
            "Expected the controller to fall back to the anonymous client id.");
    }

    private static CornController CreateController(ICornPurchaseService service, IPAddress? ipAddress = null)
    {
        var mapper = new MapperConfiguration(
            configuration => configuration.AddProfile<CornMappingProfile>(),
            NullLoggerFactory.Instance)
            .CreateMapper();

        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = ipAddress;

        return new CornController(service, mapper)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }
}
