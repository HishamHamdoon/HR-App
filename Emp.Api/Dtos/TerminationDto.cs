namespace Emp.Api.Dtos
{
    public class TerminationDto
    {
        public int Id { get; set; }
        public string TerminationType { get; set; }
        public string? TerminationReason { get; set; }
        public DateOnly DateTerminated { get; set; }
    }
}
