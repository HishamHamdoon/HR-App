namespace Emp.Api.Dtos.Auth
{
    public class LoginResponseDto
    {
        public UserDto User { get; set; }
        public string Token { get; set; }

        /// <summary>
        /// Long-lived token to obtain a new access token via POST /api/Auth/refresh.
        /// Additive: existing clients (e.g. EMP.Web) simply ignore it.
        /// </summary>
        public string? RefreshToken { get; set; }
    }
}
