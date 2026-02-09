using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TransactionService.Domain.Common;

namespace TransactionService.Domain.Entities
{
    [Table("Categories")]
    public class Category : BaseEntity
    {
        [Required]
        [StringLength(100)]
        [Column("Name")]
        public required string Name { get; set; }

        [Column("UserId")]
        public Guid? UserId { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;
    }
}
