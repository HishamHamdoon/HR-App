namespace EMP.Web.Services.Reports
{
    using Emp.Web.Dtos;
    using QuestPDF.Fluent;
    using QuestPDF.Helpers;
    using QuestPDF.Infrastructure;

    public class DepartmentReportService
    {
        public byte[] GenerateDepartmentReport(List<DepartmentDto> departments)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    // Header
                    page.Header()
                        .Text("Department Report")
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
                            columns.RelativeColumn(2);    
                            columns.ConstantColumn(50);
                        });

                        // Header row
                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCellStyle).Text("#");
                            header.Cell().Element(HeaderCellStyle).Text("Name");
                            header.Cell().Element(HeaderCellStyle).Text("Location");
                            //header.Cell().Element(HeaderCellStyle).Text("Manager");
                            
                        });

                        // Data rows
                        int index = 1;
                        foreach (var dept in departments)
                        {
                            table.Cell().Element(DataCellStyle).Text(index++);
                            table.Cell().Element(DataCellStyle).Text(dept.Name);
                            table.Cell().Element(DataCellStyle).Text(dept.Location);
                            //table.Cell().Element(DataCellStyle).Text(dept.Manager.Name);
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
