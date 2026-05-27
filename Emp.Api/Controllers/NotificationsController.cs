using Emp.Api.Data;
using Emp.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Emp.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public NotificationsController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private int CallerEmployeeId()
        {
            int.TryParse(User.FindFirst("EmployeeId")?.Value, out var id);
            return id;
        }

        /// <summary>Recent notifications for the caller plus their unread count.</summary>
        [HttpGet("mine")]
        public async Task<ResponseDto> Mine(int take = 10)
        {
            var response = new ResponseDto();
            var employeeId = CallerEmployeeId();

            var items = await _dbContext.Notifications
                .Where(n => n.RecipientEmployeeId == employeeId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(take)
                .Select(n => new { n.Id, n.Message, n.Url, n.IsRead, n.CreatedAt })
                .ToListAsync();

            var unread = await _dbContext.Notifications
                .CountAsync(n => n.RecipientEmployeeId == employeeId && !n.IsRead);

            response.IsSuccess = true;
            response.Result = new { Items = items, UnreadCount = unread };
            return response;
        }

        /// <summary>Marks every notification for the caller as read.</summary>
        [HttpPost("mark-all-read")]
        public async Task<ResponseDto> MarkAllRead()
        {
            var response = new ResponseDto();
            var employeeId = CallerEmployeeId();

            var unread = await _dbContext.Notifications
                .Where(n => n.RecipientEmployeeId == employeeId && !n.IsRead)
                .ToListAsync();
            foreach (var n in unread) n.IsRead = true;
            await _dbContext.SaveChangesAsync();

            response.IsSuccess = true;
            response.Result = unread.Count;
            return response;
        }
    }
}
