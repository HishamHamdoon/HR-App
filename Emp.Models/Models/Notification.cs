namespace Emp.Api.Models
{
    /// <summary>An in-app message addressed to a single employee (e.g. leave filed/decided).</summary>
    public class Notification
    {
        public int Id { get; set; }
        public int RecipientEmployeeId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Url { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
