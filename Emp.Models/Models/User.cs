using System.ComponentModel.DataAnnotations.Schema;

namespace Emp.Api.Models { 
    // Models/User.cs
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
    }

}
