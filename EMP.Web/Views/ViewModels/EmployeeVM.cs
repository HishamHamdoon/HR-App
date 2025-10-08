using Emp.Web.Dtos;

namespace EMP.Web.Views.ViewModels
{
    public class EmployeeVM
    {

            public int Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string Address { get; set; }
            public bool isActive { get; set; }
            public string DepartmentName { get; set; }
            public string JobTitleTitle { get; set; }
            public string CountryName { get; set; }
            public string Manager { get; set; }
            public int DepartmentId { get; set; }
            public int? SectionId { get; set; }
            public int JobTitleId { get; set; }
            public int CountryId { get; set; }
            public int? ManagerId { get; set; }
            public TerminationDto Termination { get; set; }
        // public Department Department { get; set; }

    }
}
