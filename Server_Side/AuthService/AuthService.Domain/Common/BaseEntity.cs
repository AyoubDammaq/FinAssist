using System;
using System.Collections.Generic;
using System.Text;

namespace AuthService.Domain.Common
{
    public class BaseEntity
    {
        public Guid Id { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
