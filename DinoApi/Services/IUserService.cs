
using DinoApi.Dtos;
using DinoApi.Helpers;
using Microsoft.IdentityModel.Tokens;

namespace DinoApi.Services;

public interface IUserService
{
    Task<string> RegisterUser(RegisterUserDto dataUser);
    Task<AuthResult> Login(LoginDto dataUser);
    Task<AuthResult> VerifyAndGenerateToken(TokenRequestDto tokenRequest);
}
