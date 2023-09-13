
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DinoApi.Dtos;
using DinoApi.Helpers;
using Dominio.Entities;
using Dominio.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DinoApi.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly JWT _jwt;
    private readonly TokenValidationParameters _tokenValidationParameters;
    public UserService(
        IUnitOfWork unitOfWork,
        IPasswordHasher<User> passwordHasher,
        IOptions<JWT> jwt,
        TokenValidationParameters tokenValidationParameters
    )
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwt = jwt.Value;
        _tokenValidationParameters = tokenValidationParameters;
    }
    public async Task<string> RegisterUser(RegisterUserDto dataUser)
    {
        if(VerifyUserExist(dataUser.Username)) 
        {
            User user = new()
            {
                Username = dataUser.Username,
                Email = dataUser.Email
            };
            user.Password = _passwordHasher.HashPassword(user, dataUser.Password);
            try
            {
                _unitOfWork.User.Add(user);
                await _unitOfWork.SaveAsync();
                return $"User {user.Username} has been successfully registered";

            } catch(Exception error)
            {
                return $"Error : {error.Message}";
            }
        } else 
        {
            return $"User {dataUser.Username} already exists";
        } 
    }
    public async Task<AuthResult> Login(LoginDto dataUser)
    {
        AuthResult result = new();
        if(VerifyUserExist(dataUser.Username))
        {
            result.Token = null;
            result.RefreshToken = null;
            result.Result = false;
            result.Errors.Add($"User {dataUser.Username} is not registered");
            return result;
        }
        var user = await _unitOfWork.User
                                    .GetUserByUsername(dataUser.Username);
        var passwordVerification = _passwordHasher.VerifyHashedPassword(user, user.Password, dataUser.Password);
        if(passwordVerification == PasswordVerificationResult.Success)
        {
            var securityToken = CrearTokenAsync(user);

            var refreshToken = new RefreshToken
            {
                IdUser = user.Id,
                Token = RandomStringGenerator(30),
                //JwtId = securityToken.Id,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6)
            };

            _unitOfWork.RefreshToken.Add(refreshToken);
            await _unitOfWork.SaveAsync();

            result.Token = new JwtSecurityTokenHandler().WriteToken(securityToken);
            result.RefreshToken = refreshToken.Token;
            result.Result = true;

            return result;
        } 
        else 
        {
            result.Token = null;
            result.RefreshToken = null;
            result.Result = false;
            result.Errors.Add($"Incorrect Credentials");
            return result;
        }
    }
    private SecurityToken CrearTokenAsync(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var byteKey = Encoding.UTF8.GetBytes(_jwt.Key);
        var tokenDescription = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new ("Id", user.Id.ToString()),
                new (JwtRegisteredClaimNames.Name, user.Username),
                new (JwtRegisteredClaimNames.Email, user.Email),
                new (JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString())
            }),
            Issuer = _jwt.Issuer,
            Audience = _jwt.Audience,
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(byteKey), 
                                                            SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescription);
        return token;
    }
    private bool VerifyUserExist(string username)
    {
        var userExist = _unitOfWork.User
                                   .Find(x => x.Username.ToLower() == username.ToLower())
                                   .FirstOrDefault();
        return userExist == null;
    }
    private string RandomStringGenerator(int length)
    {
        var random = new Random();
        var chars = "HS3nBaAg%=4k7x39$Gq3ELm*bR=+;&Qj$bng-82h7e.$SxP8J8";
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }
    public async Task<AuthResult> VerifyAndGenerateToken(TokenRequestDto tokenRequest)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();

        try
        {
            _tokenValidationParameters.ValidateLifetime = false;

            var tokenInVerification = jwtTokenHandler.ValidateToken(tokenRequest.Token, _tokenValidationParameters, out var validatedToken);

            if(validatedToken is JwtSecurityToken jwtSecurityToken)
            {
                var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature, StringComparison.InvariantCultureIgnoreCase);

                if(result == false) return null;
            }

            var utcExpiryDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);  
             
            var expiryDate = UnixTimeStampToDateTime(utcExpiryDate);

            if(expiryDate > DateTime.Now)
            {
                return new AuthResult() 
                {
                    Result = false,
                    Errors = new List<string>()
                    {
                        "Expired Token"
                    }
                };
            }

            var storedToken = _unitOfWork.RefreshToken.Find(x => x.Token == tokenRequest.RefreshToken)
                                                      .FirstOrDefault();

            if(storedToken == null)
            {
                return new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>()
                    {
                        "Invalid Token"
                    }
                };
            }

            if(storedToken.IsRevoked)
            {
                return new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>()
                    {
                        "Invalid Token"
                    }
                };
            }

            if(storedToken.IsUsed)
            {
                return new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>()
                    {
                        "Invalid Token"
                    }
                };
            }

            if(storedToken.ExpiryDate < DateTime.UtcNow)
            {
                return new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>()
                    {
                        "Expired Token"
                    }
                };
            }

            storedToken.IsUsed = true;
            _unitOfWork.RefreshToken.Update(storedToken);
            await _unitOfWork.SaveAsync();

            var user = _unitOfWork.User.Find(x => x.Id == storedToken.IdUser)
                                       .FirstOrDefault();

            var securityToken = CrearTokenAsync(user);

            var refreshToken = new RefreshToken
            {
                IdUser = user.Id,
                Token = RandomStringGenerator(30),
                //JwtId = securityToken.Id,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6)
            };

            _unitOfWork.RefreshToken.Add(refreshToken);
            await _unitOfWork.SaveAsync();

            var resultToken = new AuthResult
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                RefreshToken = refreshToken.Token,
                Result = true
            };

            return resultToken;


        }
        catch (Exception error)
        {
            return new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>()
                    {
                        "Server Error"
                    }
                };
        }
    }

    private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
    {
        var dateTimeVal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTimeVal = dateTimeVal.AddSeconds(unixTimeStamp).ToUniversalTime();
        return dateTimeVal; 
    }

}
