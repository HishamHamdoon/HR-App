using System.ComponentModel.DataAnnotations;

namespace EMP.Web.Views.ViewModels
{
    public class EditProfileVM
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }

    public class ChangePasswordVM
    {
        [Required, DataType(DataType.Password), Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), Display(Name = "Confirm New Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
