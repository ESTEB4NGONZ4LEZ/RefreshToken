
using System.ComponentModel.DataAnnotations;

namespace DinoApi.Dtos;

public class LoginDto
{
    [Required]
    public string Username { get; set; }
    [Required]
    public string Password { get; set; }
}
