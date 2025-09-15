using PaymentsService.DTOs;
using PaymentsService.Models;

namespace PaymentsService.Services;

public interface IPaymentRepository
{
    Task<Models.PaymentRecord> CreatePaymentAsync(CreatePaymentRequest request);
    Task<PaymentListResponse> GetPaymentsAsync(int page = 1, int pageSize = 10, string? status = null);
    Task<Models.PaymentRecord?> GetPaymentByIdAsync(Guid paymentId);
}