using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtAuthen.Services
{
    public class AuthenService : IAuthenService
    {
        private readonly DataContext dataContext;
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;

        public AuthenService(DataContext dataContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this.dataContext = dataContext;
            this.configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<User> Register(RegisterDto request)
        {
            var user = await dataContext.Users
                .FirstOrDefaultAsync(x => x.Username == request.Username);
            if (user != null) { return null; }

            string passwordHash
               = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var User = new User()
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                RoleId = request.RoleId,
            };

            await dataContext.Users.AddAsync(User);
            await dataContext.SaveChangesAsync();

            return User;
        }

        public async Task<string> Login(LoginDto request)
        {
            var user = await dataContext.Users.Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Username == request.Username);

            if (user == null) return "User not found.";

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return "Wrong password.";
            }

            string token = CreateToken(user);

            return token;
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim> {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.Name),
                };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                configuration.GetSection("AppSettings:Token").Value!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }

        public Object GetMe()
        {
            var username = string.Empty;
            var role = string.Empty;

            if (httpContextAccessor.HttpContext != null)
            {
                username = httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
                role = httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
            }
            return new { username, role };
        }

    }
}
