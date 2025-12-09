using System;
using System.Collections.Generic;
using System.Text;

namespace AuthService.Domain.Common
{
    public class BaseEntity
    {
        public Guid Id { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public DateTime? UpdatedAt { get; set; }
	}
}
