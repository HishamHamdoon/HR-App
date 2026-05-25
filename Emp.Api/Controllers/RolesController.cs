using Azure;
using Emp.Api.Dtos;
using Emp.Api.Dtos.Auth;
using Emp.Api.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace Emp.Api.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }
        [HttpGet]
        public async Task<ResponseDto> Get() 
        {
            var response = new ResponseDto();
            var roles = await _roleService.GetAllRolesAsync();
            if (roles.IsSuccess)
            {
                response.IsSuccess = true;
                response.Message = "";
                response.Result = roles;
            }
            else 
            {
                response.IsSuccess = false;
                response.Message = "Error";
                response.Result = null;
            }
            return response;
        }
        [HttpPost]
        public async Task<ResponseDto> CreateRole(string roleName)
        {
            var response = new ResponseDto();
            try
            {
                if (roleName == null)
                {
                    response.IsSuccess = false;
                    response.Result = false;
                    response.Message = "Error";
                }
                var createResponse = _roleService.CreateRoleAsync(roleName);
                response.Result = createResponse.Result;
                response.Message = string.Empty;
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Result = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}

