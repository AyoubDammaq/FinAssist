using TransactionService.Domain.Enums;

namespace TransactionService.Application.DTOs
{
    public class AddTransactionRequest
    {
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public TransactionType Type { get; set; }
    }
}
