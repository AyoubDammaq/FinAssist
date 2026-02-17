using System.ComponentModel.DataAnnotations;

namespace TransactionService.Application.DTOs
{
    public class UpdateCategoryRequest
    {
        [StringLength(100)]
        public required string Name { get; set; }
        public bool? IsActive { get; set; }
    }
}
