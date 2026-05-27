using Emp.Web.Models.Dtos;
using EMP.Web.Models.Dtos;
using EMP.Web.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EMP.Web.ViewComponents
{
    // Renders the navbar notification bell (unread count + recent items) for the signed-in user.
    public class NotificationsViewComponent : ViewComponent
    {
        private readonly INotificationService _notificationService;

        public NotificationsViewComponent(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new NotificationListDto();

            if (User.Identity?.IsAuthenticated == true)
            {
                try
                {
                    var resp = await _notificationService.GetMineAsync();
                    if (resp?.IsSuccess == true && resp.Result is not null)
                    {
                        model = JsonConvert.DeserializeObject<NotificationListDto>(Convert.ToString(resp.Result))
                                ?? new NotificationListDto();
                    }
                }
                catch
                {
                    // Bell still renders empty if the API is unreachable.
                }
            }

            return View(model);
        }
    }
}
