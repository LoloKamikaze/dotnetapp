using System;

namespace API.DTOs;

public class LoginDto
{
   public required string Username { set; get; }

   public required string Password { set; get; }
}
