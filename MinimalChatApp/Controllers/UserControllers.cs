using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using MinimalChatApp.DomainModel.Data;
using MinimalChatApp.DomainModel.Dtos.Generic;
using MinimalChatApp.DomainModel.Dtos.Incoming;
using MinimalChatApp.DomainModel.Dtos.Outgoing;
using MinimalChatApp.DomainModel.Models;
using MinimalChatApp.Util.Helpers;

namespace MinimalChatApp.Core.Controllers;

[ApiController]
[Route("api/")]
public class UserControllers : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    public UserControllers(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
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
            Password = PasswordHasher.HashPassword(model.Password),
            Token = ""
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


    [HttpPost("login")]
    public async Task<IActionResult> LoginUser([FromBody] UserLoginReqDto model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { error = "Validation failed", details = errors });
        }


        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == model.Email);

        if (user == null) { return Unauthorized(new { message = "User with this mail doesn't exists" }); }


        //Check if entered password is correct or not
        if (!PasswordHasher.VerifyPassword(model.Password, user.Password))
        {
            return Unauthorized(new { message = "Password is incorrect" });
        }

        user.Token = CreateJwtToken(user);
        var newAccessToken = user.Token;

        await _context.SaveChangesAsync();

        var response = new LoginResDto
        {
            Token = newAccessToken,
            Profile = new UserProfileDto
            {
                Id = user.Id,
                Name = user.FullName,
                Email = user.Email
            }
        };

        return Ok(response);


    }

    //private method for creating jwt token
    private string CreateJwtToken(User user)
    {
        var JwtTokenHandler = new JwtSecurityTokenHandler();
        var secret_key = _configuration["Jwt:key"];
        var key = Encoding.ASCII.GetBytes(secret_key);

        var identity = new ClaimsIdentity(new Claim[]
            {
                
                new Claim(ClaimTypes.Name, $"{user.FullName}"),
                new Claim(ClaimTypes.Email, $"{user.Email}"),

            });

        var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = identity,
            Expires = DateTime.Now.AddDays(1),
            SigningCredentials = credentials

        };

        var token = JwtTokenHandler.CreateToken(tokenDescriptor);

        return JwtTokenHandler.WriteToken(token);
    }

    private async Task<bool> CheckEmailExistAsync(string email)
    {
        return await _context.Users.AnyAsync(u=> u.Email == email);
    }
}


