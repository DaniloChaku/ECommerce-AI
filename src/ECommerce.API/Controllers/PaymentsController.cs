using ECommerce.BLL.Dtos.Orders;
using ECommerce.BLL.Dtos.Payments;
using ECommerce.BLL.Options;
using ECommerce.BLL.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using ECommerce.API.Extensions;
using ECommerce.DAL.Constants;

namespace ECommerce.API.Controllers;

[Authorize(Roles = EntityConstants.User.CustomerRole)]
public class PaymentsController : BaseApiController
{
    private readonly IPaymentService _paymentService;
    private readonly StripeConfigOptions _stripeOptions;

    public PaymentsController(
        IPaymentService paymentService,
        IOptions<StripeConfigOptions> stripeOptions)
    {
        _paymentService = paymentService;
        _stripeOptions = stripeOptions.Value;
    }

    [HttpPost("create-intent")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentIntentDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentIntentDto>> CreatePaymentIntent([FromBody] int orderId)
    {
        var userId = User.GetCurrentUserId();
        var paymentIntentDto = await _paymentService.CreatePaymentIntentAsync(orderId, userId);
        return Ok(paymentIntentDto);
    }

    [HttpPost("confirm")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> ConfirmPayment([FromBody] string paymentIntentId)
    {
        var userId = User.GetCurrentUserId();
        var orderDto = await _paymentService.ConfirmPaymentAsync(paymentIntentId, userId);
        return Ok(orderDto);
    }

    [HttpGet("status/{paymentIntentId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentStatusDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentStatusDto>> GetPaymentStatus(string paymentIntentId)
    {
        var userId = User.GetCurrentUserId();
        var paymentStatus = await _paymentService.GetPaymentStatusAsync(paymentIntentId, userId);
        return Ok(paymentStatus);
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> HandleWebhook([ModelBinder] StripeWebhookRequest request)
    {
        if (string.IsNullOrEmpty(request.JsonPayload) || string.IsNullOrEmpty(request.StripeSignature))
        {
            return BadRequest("Missing webhook data or signature");
        }

        try
        {
            var stripeEvent = ParseStripeEvent(request.JsonPayload, request.StripeSignature, _stripeOptions.WebhookSecret);

            await ProcessPaymentIntent(stripeEvent, _paymentService);

            return Ok();
        }
        catch (StripeException)
        {
            return BadRequest("Invalid webhook signature or event data");
        }
    }

    private static Event ParseStripeEvent(string jsonPayload, string stripeSignature, string webhookSecret)
    {
        return EventUtility.ConstructEvent(
            jsonPayload,
            stripeSignature,
            webhookSecret
        );
    }

    private static async Task ProcessPaymentIntent(Event stripeEvent, IPaymentService paymentService)
    {
        if (stripeEvent.Data.Object is not PaymentIntent intent)
        {
            throw new InvalidOperationException("Invalid event data: Expected PaymentIntent");
        }

        if (intent.Status == "succeeded")
        {
            if (!intent.Metadata.TryGetValue("UserId", out var userId) || string.IsNullOrEmpty(userId))
            {
                throw new InvalidOperationException("Missing UserId in payment intent metadata");
            }

            await paymentService.ConfirmPaymentAsync(intent.Id, userId);
        }
    }
}

public class StripeWebhookRequest
{
    [FromBody]
    public string JsonPayload { get; set; } = string.Empty;

    [FromHeader(Name = "Stripe-Signature")]
    public string StripeSignature { get; set; } = string.Empty;
}