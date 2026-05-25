namespace Emp.Web.Utility
{
    public class SD
    {
        // Single source of truth for the API host. Set from configuration (ApiUrls:BaseUrl) at startup.
        public static string ApiBaseUrl { get; set; } = "https://localhost:7031";
        public static string EmployeeAPIUrl{  get; set; }
        public static string DepartmentAPIUrl{  get; set; }
        public static string SectionsAPIUrl {  get; set; }
        public static string JobTitleAPIUrl {  get; set; }
        public static string CountriesAPIUrl {  get; set; }
        public static string RoleAdmin { get; set; } = "Admin";
        public static string RoleCustomer { get; set; } = "Customer";
        public static string TokenCookie { get; set; } = "TokenCookie";
        public static string Pending { get; set; } = "PENDING";
        public static string Approved { get; set; } = "APPROVED";
        public static string Rejected { get; set; } = "REJECTED";
        public static string Suspend { get; set; } = "SUSPENDED";
        public enum ApiType
        {
            Get,
            Post,
            Put,
            Delete,
            Patch
        }
        public enum ContentType
        {
            Json,
            MultiPartFormData
        }
    }
}
