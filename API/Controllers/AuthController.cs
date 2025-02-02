using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;
[Route("api/auth")]
[ApiController]
public class AuthController(AppDbContext context, ITokenService tokenAvailability ) : BaseApiController
{
    [HttpPost("inregistrare")] // inregistrare cont
    public async Task<ActionResult<UserMainDto>> Register(RegisterDto registerDto) {
        if (await UserExists(registerDto.Username)) return BadRequest("Numele de utilizator este deja luat");
       
        using var rng = RandomNumberGenerator.Create();
byte[] salt = new byte[16];
rng.GetBytes(salt);

using var pbkdf2 = new Rfc2898DeriveBytes(registerDto.Password, salt, 100000, HashAlgorithmName.SHA256);
byte[] hash = pbkdf2.GetBytes(32);

var user = new AppUser 
{
    UserName = registerDto.Username.ToLower(),
    PasswordHash = hash,
    PasswordSalt = salt
};


      context.Users.Add(user);
      await context.SaveChangesAsync();

      return new UserMainDto
      {
        Username = user.UserName,
        Token= tokenAvailability.CreateToken(user)
      };

    }

    [HttpPost("login")]

    public async Task<ActionResult<UserMainDto>> Login(LoginDto loginDto) 
    {
        var user = await context.Users.FirstOrDefaultAsync(x => 
           x.UserName == loginDto.Username.ToLower());

        if (user ==null) return Unauthorized("Username sau parola invalida");

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Parola invalida");
        }
        return new UserMainDto {
            Username = user.UserName,
            Token = tokenAvailability.CreateToken(user)
    };
    }
    private async Task<bool> UserExists(string username) {
        return await context.Users.AnyAsync(x => x.UserName.ToLower() == username.ToLower()); //Bob !=bob
    }
[HttpGet("users")] // New route: GET /api/auth/users
public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers() {
    return await context.Users.ToListAsync();
}

}
