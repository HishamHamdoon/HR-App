namespace EMP.Web.Models.Dtos
{
    public class TerminationRowDto
    {
        public string Name { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string TerminationType { get; set; } = string.Empty;
        public string TerminationReason { get; set; } = string.Empty;
        public string DateTerminated { get; set; } = string.Empty;
    }
}
