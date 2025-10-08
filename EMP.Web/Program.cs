using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Emp.Api.Data;
using Emp.Web.Utility;
using EMP.Web.Services;
using EMP.Web.Services.IServices;
using EMP.Web.Services.Reports;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuestPDF.Infrastructure;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = LicenseType.Community;

// Add localization services
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

SD.EmployeeAPIUrl = builder.Configuration["ApiUrls:EmployeeApi"];
SD.DepartmentAPIUrl = builder.Configuration["ApiUrls:DepartmentApi"];
SD.SectionsAPIUrl = builder.Configuration["ApiUrls:SectionsApi"];
SD.JobTitleAPIUrl = builder.Configuration["ApiUrls:JobTitleApi"];
SD.CountriesAPIUrl = builder.Configuration["ApiUrls:CountriesApi"];

// Localization setup
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en"),
        new CultureInfo("ar")
    };

    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    // Allow switching culture via ?culture=ar
    options.RequestCultureProviders.Insert(0, new QueryStringRequestCultureProvider());
});

// Register HttpClients + services
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<IEmployeeService, EmployeeService>();
builder.Services.AddHttpClient<ISectionService, SectionService>();
builder.Services.AddHttpClient<IJobTitleService, JobTitleService>();
builder.Services.AddHttpClient<ICountryService, CountryService>();
builder.Services.AddHttpClient<ILeaveService, LeaveService>();
builder.Services.AddHttpClient<IRoleService, RoleService>();
builder.Services.AddHttpClient<IDepartmentService, DepartmentService>();
builder.Services.AddHttpClient<IAccountService, AccountService>();
builder.Services.AddHttpClient<ISetupService, SetupService>();
builder.Services.AddHttpClient<IAuthService, AuthService>();
builder.Services.AddHttpClient<IRoleService, RoleService>();

builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ISalaryService, SalaryService>();
builder.Services.AddScoped<ILeaveService, LeaveService>();
builder.Services.AddScoped<ILeavesTypeService, LeaveTypeService>();
builder.Services.AddScoped<ISectionService, SectionService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IJobTitleService, JobTitleService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<IBaseService, BaseService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ISetupService, SetupService>();
builder.Services.AddSingleton<ITokenProvider, TokenProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddSingleton<EmployeeReportService>();
builder.Services.AddSingleton<DepartmentReportService>();


// Add MVC + Localization
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// ✅ Use Cookie Authentication
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        options.LoginPath = "/Account/Login"; // Redirect when not logged in
//        options.AccessDeniedPath = "/Account/AccessDenied"; // Redirect when forbidden
//        options.LogoutPath = "/Account/Logout";
//        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
//        options.SlidingExpiration = true;
//    });
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromHours(10);
    options.LoginPath = "/Account/login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});
//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("AdminOrEmployee", policy =>
//        policy.RequireRole("Admin", "Employee"));
//});
builder.Services.AddAuthorization();
var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Enable localization middleware
var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(locOptions);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ Authentication + Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Employees}/{action=Index}/{id?}");

//app.RunMigrations(); // optional: migration method
app.Run();
