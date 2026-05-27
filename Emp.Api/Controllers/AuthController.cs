using Azure;
using Emp.Api.Dtos;
using Emp.Api.Dtos.Auth;
using Emp.Api.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace Emp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ResponseDto _response;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
            _response = new ResponseDto();

        }
        //[HttpPost("register")]
        //public async Task<ActionResult<string>> Register([FromBody] UserDto request)
        //{
        //    try
        //    {
        //        var token = await _authService.Register(request.Username, request.Password);
        //        return Ok(token);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        //[HttpPost("login")]
        //public async Task<ActionResult<string>> Login([FromBody] UserDto request)
        //{
        //    var token = await _authService.Login(request.Username, request.Password);
        //    if (token == null) return Unauthorized("Invalid credentials");
        //    return Ok(token);
        //}
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<string>> Register([FromBody] RegisterDto model)
        {
            try
            {
               var response = await _authService.Register(model);
                if (!response.IsSuccess)
                {
                    _response.IsSuccess=false;
                    _response.Message = response.Message;
                    _response.Result = null;
                    return BadRequest(_response);
                }
                else
                {
                    return Ok(_response);
                }
               
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto model)
        {
            try
            {
                var loginResponse = await _authService.Login(model);
                if (loginResponse?.User is null)
                {
                    _response.Result = null;
                    _response.IsSuccess = false;
                    _response.Message = "Username or password is incorrect";
                    return BadRequest(_response);
                    
                }
                else
                {
                    _response.Result = loginResponse;
                    _response.IsSuccess = true;
                    return Ok(_response);
                }
               
            }
            catch (Exception ex)
            {
                _response.Result = null;
                _response.IsSuccess = false;
                _response.Message = $"An error occured:{ex.Message}";
                return StatusCode(500, _response);
               
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("role/assign")]
        public async Task<IActionResult> AssignRole(CreateRoleDto model)
        {
            var result = await _authService.AssignRole(model.Email,model.Role);
            if (result.IsSuccess)
            {
                _response.Result = true;
                _response.Message = "User assigned successfully";
                _response.IsSuccess = true;
            }
            else
            {
                _response.IsSuccess = false;
                _response.Result = null;
                _response.Message = "Something wen wrong";
            }
            return Ok(_response);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<ResponseDto> ChangePassword(Dtos.Auth.ChangePasswordDto model)
        {
            var userId = User.FindFirst("sub")?.Value
                         ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return new ResponseDto { IsSuccess = false, Message = "Not authenticated." };
            }
            return await _authService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);
        }

        [Authorize]
        [HttpPost("preferences")]
        public async Task<ResponseDto> UpdatePreferences(Dtos.Auth.UpdatePreferencesDto model)
        {
            var userId = User.FindFirst("sub")?.Value
                         ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return new ResponseDto { IsSuccess = false, Message = "Not authenticated." };
            }
            return await _authService.UpdatePreferencesAsync(userId, model.Theme, model.Calendar, model.Language);
        }
    }
}

