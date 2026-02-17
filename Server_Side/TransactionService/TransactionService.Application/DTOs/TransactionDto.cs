using TransactionService.Domain.Enums;

namespace TransactionService.Application.DTOs
{
    public class TransactionDto
    {
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public TransactionType TransactionType { get; set; }
        public Guid UserId { get; set; }
    }
}
