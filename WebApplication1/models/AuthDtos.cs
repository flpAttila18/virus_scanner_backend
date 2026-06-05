namespace WebApplication1.models
{
    // Kitöröltük a külső class AuthDtos-t, így önállóak lettek:

    public class RegisterDto // Nagy R-betűvel, hogy egyezzen a kontrollerrel!
    {
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
      
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty; // Átírtam Emailről UserName-re, mert a kontroller ezt várja bejelentkezéskor!
        public string Password { get; set; } = string.Empty;
    }
}