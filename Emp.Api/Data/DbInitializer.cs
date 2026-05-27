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

            // Rows created before LeavingDate became nullable hold the 0001-01-01 sentinel.
            // Normalise those to NULL ("still employed").
            await context.Database.ExecuteSqlRawAsync(
                "UPDATE Employees SET LeavingDate = NULL WHERE LeavingDate IS NOT NULL AND LeavingDate <= '0001-01-02'");

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

        /// <summary>
        /// Seeds test staff for every department: one manager (set as the department's
        /// manager if none exists) plus two regular employees, each with a login account
        /// (Employee role, password <see cref="DefaultUserPassword"/>). Idempotent: a
        /// department whose manager login already exists is skipped.
        /// </summary>
        public static async Task SeedTestEmployeesAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var sp = scope.ServiceProvider;

            var context = sp.GetRequiredService<AppDbContext>();
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

            var departments = await context.Departments.ToListAsync();
            if (departments.Count == 0)
            {
                return;
            }

            var country = await context.Countries.FirstOrDefaultAsync();
            if (country is null)
            {
                country = new Country { Name = "Testland", Code = "TST" };
                context.Countries.Add(country);
                await context.SaveChangesAsync();
            }

            var jobTitle = await context.JobTitles.FirstOrDefaultAsync();
            if (jobTitle is null)
            {
                jobTitle = new JobTitle { Title = "Staff", MainSalary = 1000 };
                context.JobTitles.Add(jobTitle);
                await context.SaveChangesAsync();
            }

            foreach (var dept in departments)
            {
                var managerEmail = $"mgr.dept{dept.Id}@test.com";

                // Treat the manager login as the marker for "this department is already seeded".
                if (await userManager.FindByNameAsync(managerEmail) is not null)
                {
                    continue;
                }

                var manager = new Employee
                {
                    Name = $"{dept.Name} Manager",
                    Email = managerEmail,
                    Phone = "0100000000",
                    IsActive = true,
                    BirthDate = new DateOnly(1985, 1, 1),
                    HireDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    CountryId = country.Id,
                    DepartmentId = dept.Id,
                    JobTitleId = jobTitle.Id,
                };
                context.Employees.Add(manager);
                await context.SaveChangesAsync();

                await CreateLoginAsync(userManager, manager);

                // Make this employee the department's manager if one isn't set yet.
                if (dept.ManagerId is null)
                {
                    dept.ManagerId = manager.Id;
                    await context.SaveChangesAsync();
                }

                for (var n = 1; n <= 2; n++)
                {
                    var employee = new Employee
                    {
                        Name = $"{dept.Name} Employee {n}",
                        Email = $"emp.dept{dept.Id}.{n}@test.com",
                        Phone = "0100000000",
                        IsActive = true,
                        BirthDate = new DateOnly(1995, 1, 1),
                        HireDate = DateOnly.FromDateTime(DateTime.UtcNow),
                        CountryId = country.Id,
                        DepartmentId = dept.Id,
                        JobTitleId = jobTitle.Id,
                        ManagerId = dept.ManagerId,
                    };
                    context.Employees.Add(employee);
                    await context.SaveChangesAsync();

                    await CreateLoginAsync(userManager, employee);
                }
            }
        }

        private static async Task CreateLoginAsync(UserManager<ApplicationUser> userManager, Employee employee)
        {
            var user = new ApplicationUser
            {
                UserName = employee.Email,
                Email = employee.Email,
                EmailConfirmed = true,
                PhoneNumber = employee.Phone,
                EmployeeId = employee.Id,
            };

            var result = await userManager.CreateAsync(user, DefaultUserPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, EmployeeRole);
            }
        }
    }
}
