
using System.Security.Cryptography;
using DinoApi.Dtos;
using DinoApi.Helpers;
using DinoApi.Services;
using Dominio.Entities;
using Microsoft.AspNetCore.Mvc;

namespace DinoApi.Controllers;

public class UserController : BaseApiController
{
    private readonly IUserService _userService;
    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    [HttpPost("register")]
    public async Task<ActionResult> RegisterUser(RegisterUserDto dataUser)
    {
        var register = await _userService.RegisterUser(dataUser);
        return Ok(register);
    } 
    [HttpPost("login")]
    public async Task<ActionResult<AuthResult>> Login(LoginDto dataUser)
    {
        var login = await _userService.Login(dataUser);
        return Ok(login);   
    } 
    [HttpPost("refreshToken")]
    public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto tokenRequest)
    {
        if(ModelState.IsValid)
        {
            var result = _userService.VerifyAndGenerateToken(tokenRequest);
            if(result == null)
            {
                return BadRequest(new AuthResult()
                {
                    Errors = new List<string>()
                    {
                        "Invalid Parameters"
                    },
                    Result = false
                });
            }
            else 
            {
                return Ok(result);
            }
        }
        return BadRequest(new AuthResult()
        {
            Errors = new List<string>()
            {
                "Invalid Parameters"
            },
            Result = false
        });
    }
}
