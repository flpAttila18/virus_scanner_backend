using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.models
{
    [Table("user")]
    public class User
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }

        [Required]
        [Column("email")]
        public  string Email { get; set; } = string.Empty;

        [Required]
        [Column("userName")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [Column("password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Column("role")]
        public string Role { get; set; } = string.Empty;

        [Column("profile_pic")]
        public string? Profile_Pic { get; set; } = "default.jpg";
    }
}
