using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using MinimalChatApp.DomainModel.Data;
using MinimalChatApp.DomainModel.Dtos.Incoming;
using MinimalChatApp.DomainModel.Models;
using MinimalChatApp.Util.Helpers;

namespace MinimalChatApp.Core.Controllers;

[ApiController]
[Route("api/")]
public class UserControllers : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public UserControllers(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] UserRegistrationReqDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }


        //check email
        if (await CheckEmailExistAsync(model.Email))
        {
            return Conflict(new { message = "Email already registered" });
        }

        var userToAdd = new User
        {
            Email = model.Email,
            FullName = model.Name,
            Password = PasswordHasher.HashPassword(model.Password)
        };
        await _context.Users.AddAsync(userToAdd);
        await _context.SaveChangesAsync();



        return Ok(new
        {
            userId = userToAdd.Id,
            name = userToAdd.FullName,
            email = userToAdd.Email
        });

    }

    private async Task<bool> CheckEmailExistAsync(string email)
    {
        return await _context.Users.AnyAsync(u=> u.Email == email);
    }
}


