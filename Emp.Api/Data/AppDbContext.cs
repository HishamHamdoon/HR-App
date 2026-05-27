using Emp.Api.Models;
using Emp.Models;
using Emp.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Emp.Api.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<LeavesType> LeavesTypes { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<JobTitle> JobTitles { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Termination> Terminations { get; set; }
        public DbSet<Salary> Salaries { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<License> Licenses { get; set; }
        public DbSet<CompanySettings> CompanySettings { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Salary>()
    .HasOne(s => s.Employee)
    .WithMany() // if Employee doesn’t have a collection of salaries
    .HasForeignKey(s => s.EmployeeId)
    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Employee)
                .WithOne(e => e.User)
                .HasForeignKey<ApplicationUser>(u => u.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Employee>()
        .HasOne(e => e.Country)
        .WithMany(c => c.Employees)
        .HasForeignKey(e => e.CountryId)
        .OnDelete(DeleteBehavior.Restrict);

            // Employee self-reference (manager-subordinate)
            builder.Entity<Employee>()
                .HasOne(e => e.Manager)
                .WithMany(e => e.Subordinates)
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade loops

            // Employee belongs to one department
            builder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId);

            // Department manager relationship
            builder.Entity<Department>()
                .HasOne(d => d.Manager)
                .WithMany()
                .HasForeignKey(d => d.ManagerId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade if manager is deleted

            // Sub-department self-reference
            builder.Entity<Department>()
                .HasOne(d => d.ParentDepartment)
                .WithMany(d => d.SubDepartments)
                .HasForeignKey(d => d.ParentDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Section belongs to one department
            builder.Entity<Section>()
                .HasOne(s => s.Department)
                .WithMany(d => d.Sections)
                .HasForeignKey(s => s.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Leave>()
        .HasOne(l => l.Employee)
        .WithMany()
        .HasForeignKey(l => l.EmployeeId)
        .OnDelete(DeleteBehavior.Restrict);   // or NoAction

            builder.Entity<Leave>()
                .HasOne(l => l.Manager)
                .WithMany()
                .HasForeignKey(l => l.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);   // prevent cascade path

            builder.Entity<Payroll>()
           .HasOne(p => p.Employee)
           .WithMany()
           .HasForeignKey(p => p.EmployeeId)
           .OnDelete(DeleteBehavior.Cascade); // keep cascade here

            builder.Entity<Payroll>()
                .HasOne(p => p.Salary)
                .WithMany()
                .HasForeignKey(p => p.SalaryId)
                .OnDelete(DeleteBehavior.Restrict); // 🚀 breaks cycle
            builder.Entity<Employee>()
    .HasOne(e => e.Salary)
    .WithOne(s => s.Employee)
    .HasForeignKey<Salary>(s => s.EmployeeId)
    .OnDelete(DeleteBehavior.Cascade);

            // Money columns: fixed precision to avoid silent decimal truncation.
            builder.Entity<JobTitle>().Property(j => j.MainSalary).HasPrecision(18, 2);
            builder.Entity<Salary>().Property(s => s.BasicSalary).HasPrecision(18, 2);
            builder.Entity<Salary>().Property(s => s.Allowances).HasPrecision(18, 2);
            builder.Entity<Salary>().Property(s => s.Deductions).HasPrecision(18, 2);
            builder.Entity<Payroll>().Property(p => p.GrossSalary).HasPrecision(18, 2);
            builder.Entity<Payroll>().Property(p => p.Deductions).HasPrecision(18, 2);
            builder.Entity<Payroll>().Property(p => p.NetSalary).HasPrecision(18, 2);
        }

    }
}
