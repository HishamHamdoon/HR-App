using Emp.Web.Models.Dtos;
using Emp.Web.Utility;
using EMP.Web.Models.Dtos;
using EMP.Web.Services.IServices;

namespace EMP.Web.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IBaseService _baseService;

        public NotificationService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto> GetMineAsync(int take = 10)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Get,
                Url = $"{SD.ApiBaseUrl}/api/Notifications/mine?take={take}"
            });
        }

        public async Task<ResponseDto> MarkAllReadAsync()
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.Post,
                Url = $"{SD.ApiBaseUrl}/api/Notifications/mark-all-read"
            });
        }
    }
}
