using System.Security.Claims;
using AutoFixture;
using ECommerce.API.Controllers;
using ECommerce.BLL.Dtos.Orders;
using ECommerce.BLL.Dtos.Payments;
using ECommerce.BLL.Options;
using ECommerce.BLL.ServiceContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;

namespace ECommerce.UnitTests.API.Controllers;

public class PaymentsControllerTests
{
    private readonly Mock<IPaymentService> _paymentServiceMock;
    private readonly PaymentsController _controller;
    private readonly Fixture _fixture;

    public PaymentsControllerTests()
    {
        _fixture = new Fixture();

        var stripeOptions = Options.Create(new StripeConfigOptions
        {
            WebhookSecret = "whsec_test"
        });

        _paymentServiceMock = new Mock<IPaymentService>();

        _controller = new PaymentsController(_paymentServiceMock.Object, stripeOptions);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "test-user")
                }))
            }
        };
    }

    [Fact]
    public async Task CreatePaymentIntent_ReturnsOk()
    {
        var dto = _fixture.Create<PaymentIntentDto>();
        _paymentServiceMock.Setup(p => p.CreatePaymentIntentAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(dto);

        var result = await _controller.CreatePaymentIntent(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(dto, okResult.Value);
    }

    [Fact]
    public async Task ConfirmPayment_ReturnsOk()
    {
        var dto = _fixture.Create<OrderDto>();
        _paymentServiceMock.Setup(p => p.ConfirmPaymentAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(dto);

        var result = await _controller.ConfirmPayment("pi_123");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(dto, okResult.Value);
    }

    [Fact]
    public async Task GetPaymentStatus_ReturnsOk()
    {
        var dto = _fixture.Create<PaymentStatusDto>();
        _paymentServiceMock.Setup(p => p.GetPaymentStatusAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(dto);

        var result = await _controller.GetPaymentStatus("pi_123");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(dto, okResult.Value);
    }

    [Fact]
    public async Task HandleWebhook_ReturnsBadRequest_WhenMissingSignatureOrPayload()
    {
        var result = await _controller.HandleWebhook(new StripeWebhookRequest());

        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Missing webhook data or signature", badResult.Value);
    }

    [Fact]
    public async Task HandleWebhook_ReturnsBadRequest_WhenStripeExceptionThrown()
    {
        var request = new StripeWebhookRequest
        {
            JsonPayload = "{}",
            StripeSignature = "sig"
        };

        var controller = new PaymentsController(_paymentServiceMock.Object, Options.Create(new StripeConfigOptions { WebhookSecret = "invalid" }));

        var result = await controller.HandleWebhook(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }
}