using Azure.Core;
using JwtAuthen.Models;

namespace JwtAuthen.Services
{
    public interface IAuthenService
    {
        Task<User> Register(RegisterDto request);
        Task<String> Login(LoginDto request);
        Object GetMe();

    }
}
