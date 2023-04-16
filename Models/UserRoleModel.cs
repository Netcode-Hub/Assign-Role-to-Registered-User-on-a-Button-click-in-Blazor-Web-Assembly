
using System.ComponentModel.DataAnnotations;

namespace JWTDemo.Shared.Models
{
    public class UserRoleModel
    {
        [Required]
        public string? UserId { get; set; }
        [Required]
        public string? UserName { get; set; }
        [Required]
        public string? RoleId { get; set; }
        [Required]
        public string? RoleName { get; set; }
    }
}
