using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emp.Models.Models
{
    public class BaseModel
    {
        public int Id { get; set; } // Primary Key for all models

        // Common auditing fields
        public DateTime CreatedAt { get; set; } // Timestamp when the record was created
        public DateTime UpdatedAt { get; set; } // Timestamp when the record was last updated

        // Soft delete property
        public bool IsDeleted { get; set; } // Mark whether a record is deleted or not (soft delete)

        // Optional: You could add a CreatedBy and UpdatedBy if you want to track which user made the changes
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }

        // Optional: Some additional custom functionality
        public void MarkAsDeleted()
        {
            IsDeleted = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsActive()
        {
            IsDeleted = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
