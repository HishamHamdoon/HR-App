using EMP.Web.Views.ViewModels;

namespace EMP.Web.Models
{
    public class UserProfileViewModel
    {
        public EmployeeVM Employee{ get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string EmployeeId { get; set; }
        public string Role { get; set; }
    }
}
