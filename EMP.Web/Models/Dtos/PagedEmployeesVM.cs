using EMP.Web.Views.ViewModels;

namespace EMP.Web.Models.Dtos
{
    public class PagedEmployeesVM
    {
        public List<EmployeeVM> Employees { get; set; } = new();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

}
