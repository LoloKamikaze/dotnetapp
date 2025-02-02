using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;


namespace API.Services;

public class JwtTokenService(IConfiguration config) : ITokenService
{
    public string CreateToken(AppUser user)
    {
        var jwtSecretKey = config["JwtSecret"] ?? throw new Exception("Cannot access jwt key from appsettings");
        var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecretKey)); // ASCII instead of UTF-8

        var userClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, "User") // Adding user role
        };

        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256); // SHA256 instead of SHA512

        var tokenExpiration = DateTime.UtcNow.AddMinutes(
            int.TryParse(config["JwtExpirationMinutes"], out var expiry) ? expiry : 1440 // 1 day default
        );

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(userClaims),
            Expires = tokenExpiration, 
            SigningCredentials = signingCredentials
        };

        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityTokenHandler().CreateToken(tokenDescriptor));
    }
}
