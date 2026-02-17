using System.ComponentModel.DataAnnotations;

namespace TransactionService.Application.DTOs
{
    public class AddCategoryRequest
    {
        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [Required]
        public required Guid UserId { get; set; }
    }
}
