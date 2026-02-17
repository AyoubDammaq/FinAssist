using TransactionService.Domain.Enums;

namespace TransactionService.Application.DTOs
{
    public class UpdateTransactionRequest
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public TransactionType TransactionType { get; set; }
    }
}
