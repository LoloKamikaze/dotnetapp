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
        var jwtSecretKey = config["JwtTokenKey"];
        
        if (string.IsNullOrEmpty(jwtSecretKey))
        {
            throw new Exception("JwtTokenKey is missing from configuration!");
        }

        if (jwtSecretKey.Length >= 64)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserName)
            };

            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(userClaims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = signingCredentials
            };

            var jwtHandler = new JwtSecurityTokenHandler();
            var token = jwtHandler.CreateToken(tokenDescriptor);

            return jwtHandler.WriteToken(token);
        }

        throw new Exception("Your JwtTokenKey is too short! It must be at least 64 characters.");
    }
}
