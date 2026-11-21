using SmartClinicManagement.Application.DTOs.Payments;

namespace SmartClinicManagement.Application.Interfaces;

public interface IPaymentService
{
    Task<IEnumerable<PaymentDto>> GetAllAsync();

    Task<IEnumerable<PaymentDto>> GetByBillAsync(int billId);

    Task<PaymentDto?> GetByIdAsync(int id);

    Task<PaymentDto> ReceivePaymentAsync(ReceivePaymentDto dto);
}