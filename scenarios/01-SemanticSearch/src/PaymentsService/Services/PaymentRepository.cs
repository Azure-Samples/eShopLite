using Microsoft.EntityFrameworkCore;
using PaymentsService.Data;
using PaymentsService.DTOs;
using PaymentsService.Models;
using System.Text.Json;

namespace PaymentsService.Services;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentsDbContext _context;
    private readonly ILogger<PaymentRepository> _logger;

    public PaymentRepository(PaymentsDbContext context, ILogger<PaymentRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Models.PaymentRecord> CreatePaymentAsync(CreatePaymentRequest request)
    {
        try
        {
            var paymentRecord = new Models.PaymentRecord
            {
                PaymentId = Guid.NewGuid(),
                UserId = request.UserId,
                StoreId = request.StoreId,
                CartId = request.CartId,
                Currency = request.Currency,
                Amount = request.Amount,
                Status = "Success", // Mock payment always succeeds
                PaymentMethod = request.PaymentMethod,
                ItemsJson = JsonSerializer.Serialize(request.Items),
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow
            };

            _context.Payments.Add(paymentRecord);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created payment {PaymentId} for user {UserId} with amount {Amount} {Currency}",
                paymentRecord.PaymentId, paymentRecord.UserId, paymentRecord.Amount, paymentRecord.Currency);

            return paymentRecord;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create payment for user {UserId}", request.UserId);
            throw;
        }
    }

    public async Task<PaymentListResponse> GetPaymentsAsync(int page = 1, int pageSize = 10, string? status = null)
    {
        try
        {
            var query = _context.Payments.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
            }

            var totalCount = await query.CountAsync();

            var payments = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var paymentDtos = payments.Select(p => new DTOs.PaymentRecord
            {
                PaymentId = p.PaymentId.ToString(),
                UserId = p.UserId,
                StoreId = p.StoreId,
                CartId = p.CartId,
                Currency = p.Currency,
                Amount = p.Amount,
                Status = p.Status,
                PaymentMethod = p.PaymentMethod,
                Items = JsonSerializer.Deserialize<List<PaymentItemDto>>(p.ItemsJson) ?? new(),
                CreatedAt = p.CreatedAt,
                ProcessedAt = p.ProcessedAt
            }).ToList();

            return new PaymentListResponse
            {
                Items = paymentDtos,
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get payments for page {Page}", page);
            throw;
        }
    }

    public async Task<Models.PaymentRecord?> GetPaymentByIdAsync(Guid paymentId)
    {
        try
        {
            return await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get payment {PaymentId}", paymentId);
            throw;
        }
    }
}