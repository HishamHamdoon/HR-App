namespace Emp.Api.Models
{
    //rename to leaves types
    public class LeavesType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Note   { get; set; }
        public DateTime CreatedDate { get; set;}= DateTime.Now;
        public int MaxDays{ get; set; }
        public int MinDays { get; set; } = 0;
        public bool IsAttachmentRequired { get; set; }
        public bool IsActive { get; set; }
    }
}
