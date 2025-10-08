using Emp.Web.Utility;
using EMP.Web.Services.IServices;

namespace EMP.Web.Services
{
    public class TokenProvider : ITokenProvider
    {
        private IHttpContextAccessor _contextAccessor;
        public TokenProvider(IHttpContextAccessor httpContextAccessor)
        {
            _contextAccessor = httpContextAccessor;
        }
        public void ClearToken()
        {
            _contextAccessor.HttpContext?.Response.Cookies.Delete(SD.TokenCookie);
        }

        public string? GetToken()
        {
            string token = null;
            bool? hasToekn = _contextAccessor.HttpContext?.Request.Cookies.TryGetValue(SD.TokenCookie, out token);
            return hasToekn is true ? token : null;
        }

        public void SetToken(string token)
        {
            _contextAccessor.HttpContext?.Response.Cookies.Append(SD.TokenCookie, token);
        }
    }
}
