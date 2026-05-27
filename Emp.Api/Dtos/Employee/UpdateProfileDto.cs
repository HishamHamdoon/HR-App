namespace Emp.Api.Dtos.Employee
{
    /// <summary>Fields an employee may edit on their own profile (self-service).</summary>
    public class UpdateProfileDto
    {
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }
}
