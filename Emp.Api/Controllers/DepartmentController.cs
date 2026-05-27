using ClosedXML.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Emp.Api.Data;
using Emp.Api.Dtos;
using Emp.Api.Dtos.Employee;
using Emp.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using DocumentFormat.OpenXml.Spreadsheet;
using AutoMapper;
using Emp.Api.Dtos.Department;
using Microsoft.AspNetCore.Authorization;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Emp.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase //IDocument
    {
        private readonly AppDbContext _dbContext;
        //private readonly ResponseDto _response;
        private readonly IMapper _mapper;

        private readonly List<EmployeeDto> _employees;

        public DepartmentController(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            //_response = new ResponseDto();
            _mapper = mapper;
            _employees = new List<EmployeeDto>();

        }
        // GET: api/<DepartmentController>
        [HttpGet]
        public async Task<ActionResult<ResponseDto>> Get()
        {
            var _response = new ResponseDto();
            var departments = await _dbContext.Departments.ToListAsync();
            _response.Result = _mapper.Map<List<DepartmentDto>>(departments);
            _response.IsSuccess = true;
            _response.Message = "";
            return _response;
        }

        // GET api/<DepartmentController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseDto>> Get(int id)
        {
            var _response = new ResponseDto();
            var dept = await _dbContext.Departments
                .Include(d => d.Manager)
                .Include(d => d.ParentDepartment)
                .Include(d => d.Sections)
                .Include(d => d.SubDepartments)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (dept is null)
            {
                _response.Result = null;
                _response.IsSuccess=false;
                _response.Message = "Department not found";
                return BadRequest("Department not found");
            }
            _response.Result = _mapper.Map<DepartmentDto>(dept);
            _response.IsSuccess = true;
            _response.Message = "";
            return _response;
        }

        // POST api/<DepartmentController>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ResponseDto>> Post([FromBody] DepartmentDto department)
        {
            var _response = new ResponseDto();
            if (ModelState.IsValid)
            {
                _dbContext.Departments.Add(_mapper.Map<Department>(department));
                _dbContext.SaveChanges();
                _response.IsSuccess = true;
                _response.Message = "Department Created Successfully";
                _response.Result = true;
                return _response;
            }
            else
            {
                var ErrorMessages = ModelState.Values.SelectMany(a=>a.Errors.Select(b=>b.ErrorMessage)).ToList();
                _response.IsSuccess = false;
                _response.Result=null;
                _response.Message = "Error";
                return _response;
            }
        }

        // PUT api/<DepartmentController>/5
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseDto>> Put(int id, [FromBody] Department value)
        {
            var _response = new ResponseDto();
            var targetdept = _dbContext.Departments.AsNoTracking().FirstOrDefault(x => x.Id == id);
            if (targetdept != null)
            {
                //targetdept.Location = value.Location;
                //targetdept.Name = value.Name;
                _response.IsSuccess = true;
                _response.Message = "";
                _dbContext.Update(value);
                var result = await _dbContext.SaveChangesAsync();
                _response.Result=result;
                return _response;
            }
            else
            {
                _response.Result = null;
                _response.IsSuccess=false;
                _response.Message = "Error";
                return _response;
            }
        }

        // DELETE api/<DepartmentController>/5 
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var departmentToDelete = await _dbContext.Departments.FindAsync(id);

            if (departmentToDelete == null)
            {
                return NotFound(new ResponseDto
                {
                    IsSuccess = false,
                    Message = "Department not found",
                    Result = null
                });
            }

            _dbContext.Departments.Remove(departmentToDelete);
            await _dbContext.SaveChangesAsync();

            return Ok(new ResponseDto
            {
                IsSuccess = true,
                Message = "Department deleted successfully",
                Result = null
            });
        }

        [HttpGet("GetEmployeeOfDepartments/{id}")]
        public async Task<ActionResult<ResponseDto>> GetEmployeeOfDepartments(int id)
        {
            var _response = new ResponseDto();
            try
            {
                Department? department = await _dbContext.Departments.Include(e => e.Employees).FirstOrDefaultAsync(x => x.Id == id);
                if (department is null)
                {
                    _response.Result=null;
                    _response.Message=$"Department with Id[{id}] is not found";
                    _response.IsSuccess = false;
                    return _response;
                }
                if (department.Employees is null || !department.Employees.Any())
                {
                    return NotFound($"No employees in [{department.Name}]");
                }
                var employeesDto = department.Employees.Select(e => new EmployeeDto
                {
                    DepartmentName = department.Name,
                    Name = e.Name,
                    Email = e.Email,
                    Phone = e.Phone,
                    employeeName = e.Name,
                    DepartmentId = e.DepartmentId,
                    Id = e.Id

                });
                _response.Result=employeesDto;
                _response.IsSuccess=true;
                _response.Message = "";
                return _response;
            }
            catch (Exception ex)
            {
                string message =  "An error occures while retreiving employees";
                message += ex.Message;
                _response.Message = message;
                _response.IsSuccess = false;
                _response.Result = false;
                return _response;
            }
        }

        //export employees of department into excel file
        [HttpGet("{id}/employees/export")]
        public async Task<IActionResult> ExportEmployeesToExcel(int id)
        {
            var _response = new ResponseDto();
            var department = await _dbContext.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null || department.Employees == null || !department.Employees.Any())
            {
                return NotFound("No employees found for this department.");
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Employees");

            // Header
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Phone number";
            worksheet.Cell(1, 5).Value = "Department";
            worksheet.Style.Font.FontColor = XLColor.Black;
            var headerRange = worksheet.Range("A1:E1");
            headerRange.Style.Fill.BackgroundColor = XLColor.Gray;
            headerRange.Style.Font.Bold = true;
            // Auto-size columns for just the header content
            for (int col = 1; col <= 5; col++)
            {
                worksheet.Column(col).AdjustToContents(1, 1); // From row 1 to row 1
            }

            // Data
            int row = 2;
            foreach (var emp in department.Employees)
            {
                worksheet.Cell(row, 1).Value = emp.Id;
                worksheet.Cell(row, 2).Value = emp.Name;
                worksheet.Cell(row, 3).Value = emp.Email;
                worksheet.Cell(row, 4).Value = emp.Phone;
                worksheet.Cell(row, 5).Value = emp.Department?.Name;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"Employees_Department_{department.Id}.xlsx";

            return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
        }

        //public void Compose(IDocumentContainer container)
        //{
        //    container.Page(
        //        page =>
        //        {
        //            page.Margin(30);
        //            //page.Size(pageSize:PageSize);
        //            page.Header().Text($"Employees in Department :").FontSize(20).Bold();

        //            //FontColor(ColorFilter.)
        //            page.Content()
        //       .Table(table =>
        //       {
        //           table.ColumnsDefinition(columns =>
        //           {
        //               columns.ConstantColumn(50); // ID
        //               columns.RelativeColumn();   // Name
        //           });

        //           // Header row
        //           table.Header(header =>
        //           {
        //               header.Cell().Element(CellStyle).Text("ID").Bold();
        //               header.Cell().Element(CellStyle).Text("Name").Bold();
        //           });

        //           // Data rows

        //           foreach (var emp in _employees)
        //           {
        //               table.Cell().Element(CellStyle).Text(emp.Id.ToString());
        //               table.Cell().Element(CellStyle).Text(emp.Name);
        //           }

        //           static IContainer CellStyle(IContainer container)
        //           {
        //               return container
        //                   .Padding(5)
        //                   .BorderBottom(1);
        //               //.BorderColor(Colors.Grey.Lighten2);
        //           }
        //       });

        //            page.Footer()
        //                .AlignCenter()
        //                .Text($"Generated on {DateTime.Now:g}").FontSize(10);
        //        });
        //}


        //[HttpPost("employees")]
        //public IActionResult GenerateEmployeesPdf([FromBody] List<EmployeeDto> employees)
        //{
        //    List<Employee> employeesmodel = _dbContext.Employees.ToList();
        //    employees = _mapper.Map<List<EmployeeDto>>(employeesmodel);

        //    var document = new EmployeesPdfDocument(employees);

        //    byte[] pdfBytes = document.GeneratePdf();

        //    return File(pdfBytes, "application/pdf", "employees.pdf");
        //}
        [HttpPost("employees-pdf")]
        public IActionResult GenerateEmployeesPdf([FromBody] List<EmployeeDto> employees)
        {
            try
            {
                List<Employee> employeesmodel = _dbContext.Employees.ToList();
                employees = _mapper.Map<List<EmployeeDto>>(employeesmodel);
                var document = new EmployeesPdfDocument(employees);
                byte[] pdfBytes = document.GeneratePdf();

                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    return StatusCode(500, "PDF generation failed.");
                }

                return File(pdfBytes, "application/pdf", "employees.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating PDF: {ex.Message}");
            }
        }
        /// <summary>
        /// Sets the manager for a department, its sub-departments (recursively), and every
        /// employee that belongs to any of them. Admin only.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPatch("set-manager")]
        public async Task<ResponseDto> SetManager(int departmentId, int managerId)
        {
            var response = new ResponseDto();

            if (departmentId <= 0 || managerId <= 0)
            {
                response.IsSuccess = false;
                response.Message = "Manager and department are required.";
                return response;
            }

            var managerExists = await _dbContext.Employees.AnyAsync(e => e.Id == managerId);
            if (!managerExists)
            {
                response.IsSuccess = false;
                response.Message = "Manager not found.";
                return response;
            }

            var allDepartments = await _dbContext.Departments.ToListAsync();
            if (allDepartments.All(d => d.Id != departmentId))
            {
                response.IsSuccess = false;
                response.Message = "Department not found.";
                return response;
            }

            // Walk the department + all descendant sub-departments.
            var targetIds = new HashSet<int>();
            var queue = new Queue<int>();
            queue.Enqueue(departmentId);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (!targetIds.Add(current)) continue;
                foreach (var child in allDepartments.Where(d => d.ParentDepartmentId == current))
                    queue.Enqueue(child.Id);
            }

            // Set the manager on each department in the subtree.
            foreach (var dept in allDepartments.Where(d => targetIds.Contains(d.Id)))
                dept.ManagerId = managerId;

            // Set the manager on every employee in those departments (the manager isn't their own manager).
            var employees = await _dbContext.Employees
                .Where(e => targetIds.Contains(e.DepartmentId))
                .ToListAsync();
            var affected = 0;
            foreach (var emp in employees)
            {
                if (emp.Id == managerId) continue;
                emp.ManagerId = managerId;
                affected++;
            }

            await _dbContext.SaveChangesAsync();

            response.IsSuccess = true;
            response.Result = new { Departments = targetIds.Count, Employees = affected };
            response.Message = $"Manager assigned to {targetIds.Count} department(s) and {affected} employee(s).";
            return response;
        }

        /// <summary>
        /// Removes the manager from a department, its sub-departments, and clears that manager
        /// from every employee in them. Admin only.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPatch("remove-manager")]
        public async Task<ResponseDto> RemoveManager(int departmentId)
        {
            var response = new ResponseDto();
            if (departmentId <= 0)
            {
                response.IsSuccess = false;
                response.Message = "Department is required.";
                return response;
            }

            var allDepartments = await _dbContext.Departments.ToListAsync();
            if (allDepartments.All(d => d.Id != departmentId))
            {
                response.IsSuccess = false;
                response.Message = "Department not found.";
                return response;
            }

            var targetIds = new HashSet<int>();
            var queue = new Queue<int>();
            queue.Enqueue(departmentId);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (!targetIds.Add(current)) continue;
                foreach (var child in allDepartments.Where(d => d.ParentDepartmentId == current))
                    queue.Enqueue(child.Id);
            }

            // Capture the managers being removed so we only clear employees that pointed at them.
            var removedManagerIds = allDepartments
                .Where(d => targetIds.Contains(d.Id) && d.ManagerId.HasValue)
                .Select(d => d.ManagerId!.Value)
                .ToHashSet();

            foreach (var dept in allDepartments.Where(d => targetIds.Contains(d.Id)))
                dept.ManagerId = null;

            var employees = await _dbContext.Employees
                .Where(e => targetIds.Contains(e.DepartmentId) && e.ManagerId.HasValue && removedManagerIds.Contains(e.ManagerId.Value))
                .ToListAsync();
            foreach (var emp in employees)
                emp.ManagerId = null;

            await _dbContext.SaveChangesAsync();

            response.IsSuccess = true;
            response.Result = true;
            response.Message = "Manager removed.";
            return response;
        }

        [HttpGet("sections/{departmentId}")]
        public async Task<ResponseDto> GetSectionsByDepartment(int departmentId)
        {
            var response = new ResponseDto();
             
            try
            {
                var sections = await _dbContext.Sections
                                           .Where(s => s.DepartmentId == departmentId)
                                           .Select(s => new { s.Id, s.Name })
                                           .ToListAsync();
                response.IsSuccess = true;
                response.Result = sections;
                response.Message = "";
                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Result = null;
                response.Message = ex.Message;
                return response;
            }
        }
    }
}