namespace JwtAuthen.Models
{
    public class RegisterDto
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required int RoleId { get; set; } 
    }
}
