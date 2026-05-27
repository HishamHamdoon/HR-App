using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Emp.Api.Data;
using Emp.Web.Utility;
using EMP.Web.Services;
using EMP.Web.Services.IServices;
using EMP.Web.Services.Reports;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
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

// Single API host from config; all endpoint URLs are derived from it.
SD.ApiBaseUrl = (builder.Configuration["ApiUrls:BaseUrl"] ?? "https://localhost:7031").TrimEnd('/');
SD.EmployeeAPIUrl = $"{SD.ApiBaseUrl}/api/Employee";
SD.DepartmentAPIUrl = $"{SD.ApiBaseUrl}/api/Department";
SD.SectionsAPIUrl = $"{SD.ApiBaseUrl}/api/Sections";
SD.JobTitleAPIUrl = $"{SD.ApiBaseUrl}/api/JobTitle";
SD.CountriesAPIUrl = $"{SD.ApiBaseUrl}/api/Countries";

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
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ILicenseService, LicenseService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ISetupService, SetupService>();
builder.Services.AddSingleton<ITokenProvider, TokenProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRoleService, RoleService>();


// Persist Data Protection keys so the auth cookie stays decryptable across app
// restarts. Without this the key ring is regenerated on each start, old cookies
// become invalid, and users appear "logged in but not authorized" until they
// sign in a second time.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys")))
    .SetApplicationName("EmpWeb");

// Add MVC + Localization
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization()
    // The Web references Emp.Api for shared types (BaseController, DTOs, models). That
    // assembly also contains the API controllers, which ASP.NET would otherwise discover
    // and host here — they can't resolve their API-only services and their attribute
    // routes (e.g. /api/License/activate) collide with our MVC link generation. Host only
    // this app's own controllers.
    .ConfigureApplicationPartManager(apm =>
    {
        var apiPart = apm.ApplicationParts
            .FirstOrDefault(p => p.Name.Equals("Emp.Api", StringComparison.OrdinalIgnoreCase));
        if (apiPart is not null)
        {
            apm.ApplicationParts.Remove(apiPart);
        }
    });

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
    // Secure when the request is HTTPS, relaxed on plain HTTP. This lets the app run on an
    // internal LAN over HTTP and stay secure behind a TLS-terminating reverse proxy.
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Honour X-Forwarded-* from a reverse proxy (TLS termination) so HTTPS is detected correctly.
builder.Services.Configure<Microsoft.AspNetCore.Builder.ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
                               | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
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

// Trust reverse-proxy forwarded headers (must run before auth/redirect logic).
app.UseForwardedHeaders();

// Enable localization middleware
var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(locOptions);

// In containers TLS is handled by the reverse proxy; in-app redirection would loop.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

// ✅ Authentication + Authorization
app.UseAuthentication();
app.UseAuthorization();

// Lock the app when the license is expired (after auth so we know who the user is).
app.UseMiddleware<EMP.Web.Middleware.LicenseEnforcementMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.RunMigrations(); // optional: migration method
app.Run();
