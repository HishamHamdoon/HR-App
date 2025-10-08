namespace Emp.Api.Dtos.Vacation
{
    public class LeaveTypesViewDto
    {
        public int Id { get; set; }
        public string Note { get; set; }
        public int MaxDays { get; set; }
        public int MinDays { get; set; } = 0;
        //public int EmployeeId { get; set; }
        public string Name { get; set; }
        public bool IsAttachmentRequired { get; set; }
        public bool IsActive { get; set; }
    }
}
