using EMP.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Emp.Api.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var token = HttpContext.Request.Cookies["jwt"];
            if (!string.IsNullOrEmpty(token))
            {
                var username = JwtHelper.GetUsernameFromJwt(token);
                ViewBag.Username = username;
            }
            base.OnActionExecuting(context);
        }
    }
}
