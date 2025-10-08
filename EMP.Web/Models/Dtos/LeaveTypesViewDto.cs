namespace EMP.Web.Models.Dtos
{
    public class LeaveTypesViewDto
    {
        public int Id { get; set; }
        public string Note { get; set; }
        public int MaxDays { get; set; }
        public int MinDays { get; set; } = 0;
        public string Name { get; set; }
        public bool IsAttachmentRequired { get; set; }
        public bool IsActive { get; set; }
    }


    public class PaggedLeavTypeVM
    {
        public List<LeaveTypesViewDto> LeaveTypes { get; set; } = new();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
