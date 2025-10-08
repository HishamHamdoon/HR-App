using Emp.Api.Models;

namespace Emp.Api.Dtos.Leave
{
    public class UpdateLeaveTypesDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public int MaxDays { get; set; }
        public int MinDays { get; set; } = 0;
        public bool IsAttachmentRequired { get; set; }
        public bool IsActive { get; set; }
        //public int LeaveTypeId { get; set; }

    }
}
