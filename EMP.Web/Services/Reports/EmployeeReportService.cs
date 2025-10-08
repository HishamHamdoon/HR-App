namespace EMP.Web.Services.Reports
{
    using QuestPDF.Fluent;
    using QuestPDF.Helpers;
    using QuestPDF.Infrastructure;
    using EMP.Web.Models.Dtos; // adjust namespace for your Employee DTOs
    using Emp.Api.Dtos.Employee;

    public class EmployeeReportService
    {
        public byte[] GenerateEmployeeReport(List<EmployeeViewDto> employees)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    // Header
                    page.Header()
                        .Text("Employee Report")
                        .FontSize(20)
                        .Bold()
                        .AlignCenter();

                    // Content (table of employees)
                    page.Content().Table(table =>
                    {
                        // Define columns
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);     // #
                            columns.RelativeColumn(2);      // Name
                            columns.RelativeColumn(3);      // Email
                            columns.RelativeColumn(2);      // Department
                            columns.RelativeColumn(2);      // Position
                            columns.ConstantColumn(50);
                        });

                        // Header row
                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCellStyle).Text("#");
                            header.Cell().Element(HeaderCellStyle).Text("Name");
                            header.Cell().Element(HeaderCellStyle).Text("Email");
                            header.Cell().Element(HeaderCellStyle).Text("Department");
                            header.Cell().Element(HeaderCellStyle).Text("Position");
                            header.Cell().Element(HeaderCellStyle).Text("Status");
                        });

                        // Data rows
                        int index = 1;
                        foreach (var emp in employees)
                        {
                            table.Cell().Element(DataCellStyle).Text(index++);
                            table.Cell().Element(DataCellStyle).Text(emp.Name);
                            table.Cell().Element(DataCellStyle).Text(emp.Email);
                            table.Cell().Element(DataCellStyle).Text(emp.DepartmentName ?? "-");
                            table.Cell().Element(DataCellStyle).Text(emp.JobTitleTitle ?? "-");
                            table.Cell().Element(DataCellStyle).Text(emp.isActive.ToString());
                        }

                        // Styles
                        static IContainer HeaderCellStyle(IContainer container) =>
                            container
                                .PaddingVertical(5)
                                .PaddingHorizontal(3)
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Medium)
                                .DefaultTextStyle(x => x.SemiBold());

                        static IContainer DataCellStyle(IContainer container) =>
                            container
                                .PaddingVertical(4)
                                .PaddingHorizontal(3)
                                .BorderBottom(0.5f)
                                .BorderColor(Colors.Grey.Lighten2)
                                .AlignMiddle(); // aligns text vertically
                    });

                    // Footer
                    page.Footer()
                        .AlignCenter()
                        .Text($"Generated on {DateTime.Now:yyyy-MM-dd HH:mm}");
                });
            });

            return document.GeneratePdf();
        }
    }

}
