using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenAvailability;

    public AuthController(AppDbContext context, ITokenService tokenAvailability)
    {
        _context = context;
        _tokenAvailability = tokenAvailability;
    }

    // 🔹 Register a new user using JSON body
    [HttpPost("inregistrare")]
    public async Task<ActionResult<UserMainDto>> Register([FromBody] RegisterDto registerDto) 
    {
        if (await UserExists(registerDto.Username)) 
            return BadRequest("Numele de utilizator este deja luat");

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

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new UserMainDto
        {
            Username = user.UserName,
            Token = _tokenAvailability.CreateToken(user)
        };
    }

    // 🔹 Register a new user using Query Parameters
    [HttpPost("register")]
    public async Task<ActionResult<UserMainDto>> RegisterQuery([FromQuery] string username, [FromQuery] string password)
    {
        if (await UserExists(username)) 
            return BadRequest("Numele de utilizator este deja luat");

        using var rng = RandomNumberGenerator.Create();
        byte[] salt = new byte[16];
        rng.GetBytes(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(32);

        var user = new AppUser
        {
            UserName = username.ToLower(),
            PasswordHash = hash,
            PasswordSalt = salt
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new UserMainDto
        {
            Username = user.UserName,
            Token = _tokenAvailability.CreateToken(user)
        };
    }

    // 🔹 Login User
    [HttpPost("login")]
    public async Task<ActionResult<UserMainDto>> Login([FromBody] LoginDto loginDto) 
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());

        if (user == null) 
            return Unauthorized("Username sau parola invalida");

        using var pbkdf2 = new Rfc2898DeriveBytes(loginDto.Password, user.PasswordSalt, 100000, HashAlgorithmName.SHA256);
        byte[] computedHash = pbkdf2.GetBytes(32);

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i]) 
                return Unauthorized("Parola invalida");
        }

        return new UserMainDto 
        {
            Username = user.UserName,
            Token = _tokenAvailability.CreateToken(user)
        };
    }

    // 🔹 Get all users
    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers() 
    {
        return await _context.Users.ToListAsync();
    }

    // 🔹 Get a user by ID
    [HttpGet("user/{id}")]
    public async Task<ActionResult<AppUser>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) 
            return NotFound();
        return user;
    }

    // 🔹 Check if a username exists
    private async Task<bool> UserExists(string username) 
    {
        return await _context.Users.AnyAsync(x => x.UserName.ToLower() == username.ToLower());
    }
}
