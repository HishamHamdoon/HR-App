using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using System;
using System.Collections.Generic;
using QuestPDF.Drawing;
using Emp.Api.Dtos.Employee;

public class EmployeesPdfDocument : IDocument
{
    private readonly List<EmployeeDto> _employees;

    public EmployeesPdfDocument(List<EmployeeDto> employees)
    {
        _employees = employees;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(30);

            page.Header()
                .Text("Employees in Department")
                .FontSize(20)
                .Bold();

            page.Content()
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(50);
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("ID").Bold();
                        header.Cell().Element(CellStyle).Text("Name").Bold();
                    });

                    foreach (var emp in _employees)
                    {
                        table.Cell().Element(CellStyle).Text(emp.Id.ToString());
                        table.Cell().Element(CellStyle).Text(emp.Name);
                    }

                    static IContainer CellStyle(IContainer container)
                    {
                        return container
                            .Padding(5)
                            .BorderBottom(1);
                    }
                });

            page.Footer()
                .AlignCenter()
                .Text($"Generated on {DateTime.Now:g}")
                .FontSize(10);
        });
    }
}
