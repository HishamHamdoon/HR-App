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
            CreateMap<Employee, EmployeeViewDto>().ForMember(dist => dist.DepartmentName,
                opt => opt.MapFrom(src => src.Department .Name)).ReverseMap();
            CreateMap<Employee, EmployeeViewDto>().ForMember(dist => dist.JobTitleTitle,
                opt => opt.MapFrom(src => src.JobTitle.Title)).ReverseMap();
            CreateMap<Employee, EmployeeViewDto>().ForMember(dist => dist.CountryName,
                opt => opt.MapFrom(src => src.Country.Name)).ReverseMap();
            CreateMap<Employee, EmployeeViewDto>().ForMember(dist => dist.Manager,
               opt => opt.MapFrom(src => src.Manager.Name)).ReverseMap();
            //CreateMap<Employee, EmployeeViewDto>().ForMember(dist => dist.employeeName,
            //    opt => opt.MapFrom(src => src.employee.Name)).ReverseMap();
            CreateMap<Employee, EmployeeUpdateDto>().ReverseMap();
            //end of map employee 
            //start of map Department
            CreateMap<Department, DepartmentDto>().ReverseMap();
            //end of map Department
            //start of map Leave
            CreateMap<Leave, CreateLeaveDto>().ReverseMap();
            CreateMap<Leave, UpdateLeaveDto>().ReverseMap();

            CreateMap<Leave, ViewLeaveDto>()
                .ForMember(dest => dest.EmployeeName,
                           opt => opt.MapFrom(src => src.Employee.Name))
                .ForMember(dest => dest.LeaveName,
                           opt => opt.MapFrom(src => src.LeavesType.Name))
                .ForMember(dest => dest.ManagerName,
                           opt => opt.MapFrom(src => src.Manager.Name))
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
            CreateMap<Section,SectionViewDto>().ForMember(dist => dist.DepartmenName, opt => opt.MapFrom(src => src.Department.Name)).ReverseMap();
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

