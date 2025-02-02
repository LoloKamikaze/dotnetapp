using System;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;
public class UsersManagementController(AppDbContext context) : BaseApiController
{

    [HttpGet("all")]
public async Task<IActionResult> FetchAllUsers()
{
    var users = await context.Users.ToListAsync();
    return users.Any() ? Ok(users) : NotFound("No users found.");
}

  [AllowAnonymous]
  [HttpGet("{id:int}")]  // api/users/idofuser
    public async Task<ActionResult<AppUser>> GetUser(int id)
    {
        var user = await context.Users.FindAsync(id);

        if (user == null) return NotFound();

        return user;
    }
}

