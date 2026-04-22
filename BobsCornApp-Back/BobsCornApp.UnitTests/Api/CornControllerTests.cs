using AutoMapper;
using BobsCornApp.Api.Controllers;
using BobsCornApp.Application.Dtos;
using BobsCornApp.Application.Interfaces;
using BobsCornApp.Application.Mapping;
using BobsCornApp.Application.Options;
using BobsCornApp.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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
            && dto.Message == "Rate limit exceeded. Clients can buy at most 1 corn every 60 seconds."
            && controller.Response.Headers.RetryAfter == "8",
            "Expected a 429 response with a rounded retry-after header and payload.");
    }

    [TestMethod]
    public async Task BuyCornShouldReturnBadRequestWhenClientIdIsMissing()
    {
        var service = new Mock<ICornPurchaseService>();
        var controller = CreateController(service.Object);

        var result = await controller.BuyCorn(null, CancellationToken.None);

        Assert.IsTrue(
            result is BadRequestObjectResult { Value: BaseResponseDto dto }
            && dto.Message == "The clientId query parameter is required."
            && service.Invocations.Count == 0,
            "Expected a 400 response when the clientId query parameter is missing.");
    }

    [TestMethod]
    public async Task BuyCornShouldReturnBadRequestWhenClientIdIsBlank()
    {
        var service = new Mock<ICornPurchaseService>();
        var controller = CreateController(service.Object);

        var result = await controller.BuyCorn("   ", CancellationToken.None);

        Assert.IsTrue(
            result is BadRequestObjectResult { Value: BaseResponseDto dto }
            && dto.Message == "The clientId query parameter is required."
            && service.Invocations.Count == 0,
            "Expected a 400 response when the clientId query parameter is blank.");
    }

    private static CornController CreateController(ICornPurchaseService service)
    {
        var mapper = new MapperConfiguration(
            configuration => configuration.AddProfile<CornMappingProfile>(),
            NullLoggerFactory.Instance)
            .CreateMapper();

        return new CornController(
            service,
            mapper,
            Options.Create(new CornRateLimitOptions { WindowSeconds = 60 }))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }
}
