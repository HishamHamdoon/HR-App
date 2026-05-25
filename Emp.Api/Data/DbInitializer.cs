using Emp.Api.Models;
using Emp.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Emp.Api.Data
{
    public static class DbInitializer
    {
        public const string AdminRole = "Admin";
        public const string EmployeeRole = "Employee";

        // Default password applied to newly-registered employees when none is supplied.
        public const string DefaultUserPassword = "P@ssw0rd";

        public const string AdminUserName = "admin";
        public const string AdminEmail = "admin@admin.com";
        public const string AdminPassword = "admin";

        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var sp = scope.ServiceProvider;

            var context = sp.GetRequiredService<AppDbContext>();
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.MigrateAsync();

            foreach (var role in new[] { AdminRole, EmployeeRole })
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            if (await userManager.FindByNameAsync(AdminUserName) is not null)
            {
                return;
            }

            var country = await context.Countries.FirstOrDefaultAsync();
            if (country is null)
            {
                country = new Country { Name = "System", Code = "SYS" };
                context.Countries.Add(country);
                await context.SaveChangesAsync();
            }

            var department = await context.Departments.FirstOrDefaultAsync();
            if (department is null)
            {
                department = new Department { Name = "System" };
                context.Departments.Add(department);
                await context.SaveChangesAsync();
            }

            var jobTitle = await context.JobTitles.FirstOrDefaultAsync();
            if (jobTitle is null)
            {
                jobTitle = new JobTitle { Title = "Super Admin", MainSalary = 0 };
                context.JobTitles.Add(jobTitle);
                await context.SaveChangesAsync();
            }

            var adminEmployee = await context.Employees.FirstOrDefaultAsync(e => e.Email == AdminEmail);
            if (adminEmployee is null)
            {
                adminEmployee = new Employee
                {
                    Name = "Super Admin",
                    Email = AdminEmail,
                    Phone = "0000000000",
                    IsActive = true,
                    BirthDate = new DateOnly(1990, 1, 1),
                    HireDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    CountryId = country.Id,
                    DepartmentId = department.Id,
                    JobTitleId = jobTitle.Id,
                };
                context.Employees.Add(adminEmployee);
                await context.SaveChangesAsync();
            }

            var adminUser = new ApplicationUser
            {
                UserName = AdminUserName,
                Email = AdminEmail,
                EmailConfirmed = true,
                PhoneNumber = "0000000000",
                EmployeeId = adminEmployee.Id,
            };

            // Hash the password directly so the seed bypasses the runtime password policy.
            // This keeps the test admin (admin/admin) while still enforcing strong passwords for real users.
            var hasher = sp.GetRequiredService<IPasswordHasher<ApplicationUser>>();
            adminUser.PasswordHash = hasher.HashPassword(adminUser, AdminPassword);

            var createResult = await userManager.CreateAsync(adminUser);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to seed admin user: {errors}");
            }

            await userManager.AddToRoleAsync(adminUser, AdminRole);
        }
    }
}
