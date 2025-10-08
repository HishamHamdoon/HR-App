using Emp.Api.Models;

namespace EMP.Web.Views.ViewModels
{
    public class EmployeeFormViewModel
    {
        public Employee Employee { get; set; } = new Employee();

        public List<Department> Departments { get; set; } = new();
        public List<Section> Sections { get; set; } = new();
        public List<JobTitle> JobTitles { get; set; } = new();
        public List<Country> Countries { get; set; } = new();


    }
}
