using Microsoft.AspNetCore.Mvc;
using PaymentsService.DTOs;
using PaymentsService.Services;

namespace PaymentsService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentRepository paymentRepository, ILogger<PaymentsController> logger)
    {
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<CreatePaymentResponse>> CreatePayment([FromBody] CreatePaymentRequest request)
    {
        try
        {
            // Validate request
            if (string.IsNullOrEmpty(request.UserId))
            {
                return BadRequest("UserId is required");
            }

            if (string.IsNullOrEmpty(request.Currency))
            {
                return BadRequest("Currency is required");
            }

            if (request.Amount <= 0)
            {
                return BadRequest("Amount must be greater than 0");
            }

            if (string.IsNullOrEmpty(request.PaymentMethod))
            {
                return BadRequest("PaymentMethod is required");
            }

            if (request.Items?.Count == 0)
            {
                return BadRequest("Items are required");
            }

            // Create payment
            var paymentRecord = await _paymentRepository.CreatePaymentAsync(request);

            var response = new CreatePaymentResponse
            {
                PaymentId = paymentRecord.PaymentId.ToString(),
                Status = paymentRecord.Status,
                ProcessedAt = paymentRecord.ProcessedAt ?? DateTime.UtcNow
            };

            _logger.LogInformation("Payment {PaymentId} created successfully for user {UserId}",
                response.PaymentId, request.UserId);

            return CreatedAtAction(nameof(GetPayment), new { id = response.PaymentId }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create payment for user {UserId}", request.UserId);
            return StatusCode(500, "Internal server error occurred while processing payment");
        }
    }

    [HttpGet]
    public async Task<ActionResult<PaymentListResponse>> GetPayments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var payments = await _paymentRepository.GetPaymentsAsync(page, pageSize, status);
            return Ok(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get payments for page {Page}", page);
            return StatusCode(500, "Internal server error occurred while retrieving payments");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CreatePaymentResponse>> GetPayment(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var paymentId))
            {
                return BadRequest("Invalid payment ID format");
            }

            var payment = await _paymentRepository.GetPaymentByIdAsync(paymentId);
            if (payment == null)
            {
                return NotFound();
            }

            var response = new CreatePaymentResponse
            {
                PaymentId = payment.PaymentId.ToString(),
                Status = payment.Status,
                ProcessedAt = payment.ProcessedAt ?? DateTime.UtcNow
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get payment {PaymentId}", id);
            return StatusCode(500, "Internal server error occurred while retrieving payment");
        }
    }
}