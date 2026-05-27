using AutoMapper;
using Emp.Api.Dtos;
using Emp.Api.Dtos.Department;
using Emp.Api.Dtos.Employee;
using Emp.Api.Dtos.JobTitleDto;
using Emp.Api.Dtos.Leave;
using Emp.Api.Dtos.Models;
using Emp.Api.Dtos.Salary;
using Emp.Api.Dtos.Section;
using Emp.Api.Dtos.Vacation;
using Emp.Api.Models;
using Emp.Models;
using Emp.Models.Models;

namespace Emp.Api
{
    public class MappingProfile : Profile
    {
    public MappingProfile()
        {
            // Add as many of these lines as you need to map your objects
            //start of map employee
            CreateMap<Employee, EmployeeDto>().ReverseMap();
            CreateMap<Employee, EmployeeCreateDto>().ReverseMap();
            CreateMap<Employee, EmployeeViewDto>()
                .ForMember(d => d.DepartmentName, opt => opt.MapFrom(s => s.Department != null ? s.Department.Name : null))
                .ForMember(d => d.JobTitleTitle, opt => opt.MapFrom(s => s.JobTitle != null ? s.JobTitle.Title : null))
                .ForMember(d => d.CountryName, opt => opt.MapFrom(s => s.Country != null ? s.Country.Name : null))
                .ForMember(d => d.Manager, opt => opt.MapFrom(s => s.Manager != null ? s.Manager.Name : null))
                .ReverseMap();
            //CreateMap<Employee, EmployeeViewDto>().ForMember(dist => dist.employeeName,
            //    opt => opt.MapFrom(src => src.employee.Name)).ReverseMap();
            CreateMap<Employee, EmployeeUpdateDto>().ReverseMap();
            //end of map employee 
            //start of map Department
            CreateMap<Section, NamedItemDto>();
            CreateMap<Department, NamedItemDto>();
            CreateMap<Department, DepartmentDto>()
                .ForMember(d => d.ParentDepartmentName, o => o.MapFrom(s => s.ParentDepartment != null ? s.ParentDepartment.Name : null))
                .ForMember(d => d.ManagerName, o => o.MapFrom(s => s.Manager != null ? s.Manager.Name : null))
                .ReverseMap()
                .ForMember(d => d.Sections, o => o.Ignore())
                .ForMember(d => d.SubDepartments, o => o.Ignore())
                .ForMember(d => d.ParentDepartment, o => o.Ignore())
                .ForMember(d => d.Manager, o => o.Ignore())
                .ForMember(d => d.Employees, o => o.Ignore());
            //end of map Department
            //start of map Leave
            CreateMap<Leave, CreateLeaveDto>().ReverseMap();
            CreateMap<Leave, UpdateLeaveDto>().ReverseMap();

            CreateMap<Leave, ViewLeaveDto>()
                .ForMember(d => d.EmployeeName, opt => opt.MapFrom(s => s.Employee != null ? s.Employee.Name : null))
                .ForMember(d => d.LeaveName, opt => opt.MapFrom(s => s.LeavesType != null ? s.LeavesType.Name : null))
                .ForMember(d => d.ManagerName, opt => opt.MapFrom(s => s.Manager != null ? s.Manager.Name : null))
                .ReverseMap();
            
            //end of map Leave
            //start of map LeavesType
            //CreateMap<Leave, CreateLeaveDto>().ReverseMap();
            //CreateMap<Leave, UpdateLeaveDto>().ReverseMap();
            //CreateMap<LeavesType, ViewDto>().ForMember(dist=>dist.EmployeeName, opt=>opt.MapFrom(src=>src.Employee.Name)).ReverseMap();
            //CreateMap<LeavesType, ViewDto>().ForMember(dist=>dist.LeaveName, opt=>opt.MapFrom(src=>src.Leave.Name)).ReverseMap();
            //end of map LeavesType
            //strat of map job title
            CreateMap<JobTitle,JobTitleCreateDto>().ReverseMap();
            CreateMap<JobTitle,JobTitleUpdateDto>().ReverseMap();
            CreateMap<JobTitle,JobTitleViewDto>().ReverseMap();
            //end of map job title

            //start map leavetypes
            CreateMap<LeavesType, LeaveTypesViewDto>().ReverseMap();
            CreateMap<LeavesType, CreateLeaveTypesDto>().ReverseMap();
            CreateMap<LeavesType, UpdateLeaveTypesDto>().ReverseMap();

            //CreateMap<LeavesType, LeaveTypesViewDto>().ForMember(dist => dist.EmployeeName, opt => opt.MapFrom(src => src.Employee.Name)).;
            //end map leavetypes
            //start map employee
            CreateMap<Country, CountryCreateDto>().ReverseMap();

            //end map employee
            //start map section
            CreateMap<Section,SectionCreateDto>().ReverseMap();
            CreateMap<Section,SectionUpdateDto>().ReverseMap();
            CreateMap<Section, SectionViewDto>()
                .ForMember(d => d.DepartmenName, opt => opt.MapFrom(s => s.Department != null ? s.Department.Name : null))
                .ReverseMap();
            ;
            CreateMap<CreateSalaryDto, Salary>();
            CreateMap<UpdateSalaryDto, Salary>();
            CreateMap<Salary, SalaryDto>()
     .ForMember(dest => dest.EmployeeName,
                opt => opt.MapFrom(src => src.Employee != null ? src.Employee.Name : string.Empty));
            // Entity → DTO
            CreateMap<Payroll, PayrollDto>()
                .ForMember(dest => dest.EmployeeName,
                           opt => opt.MapFrom(src => src.Employee != null ? src.Employee.Name : string.Empty));

            // DTO → Entity (useful if you post new payrolls from frontend)
            CreateMap<PayrollDto, Payroll>();
        }
    }
}

