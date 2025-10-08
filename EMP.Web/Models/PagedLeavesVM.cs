using Emp.Web.Dtos;

namespace EMP.Web.Models
{
    public class PagedLeavesVM
    {
        public List<ViewLeaveDto> Leaves { get; set; } = new();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
