using Emp.Web.Models.Dtos;

namespace EMP.Web.Services.IServices
{
    public interface INotificationService
    {
        Task<ResponseDto> GetMineAsync(int take = 10);
        Task<ResponseDto> MarkAllReadAsync();
    }
}
