using Emp.Api.Models;
using Microsoft.AspNetCore.Identity;


namespace Emp.Models.Models
{
    

        public class ApplicationUser : IdentityUser
        {
            public int EmployeeId { get; set; }
            public Employee Employee { get; set; }
        }
    

}
