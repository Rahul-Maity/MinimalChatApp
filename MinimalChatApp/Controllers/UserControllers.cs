using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using MinimalChatApp.DomainModel.Data;
using MinimalChatApp.DomainModel.Dtos.Generic;
using MinimalChatApp.DomainModel.Dtos.Incoming;


using MinimalChatApp.DomainModel.Dtos.Outgoing;
using MinimalChatApp.DomainModel.Models;
using MinimalChatApp.Repository.Users;
using MinimalChatApp.Util.Helpers;

namespace MinimalChatApp.Core.Controllers;

[ApiController]
[Route("api/")]
public class UserControllers : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    private readonly IUserRepository _userRepository;
    public UserControllers(ApplicationDbContext context, IConfiguration configuration,IUserRepository userRepository)
    {
        _context = context;
        _configuration = configuration;
        _userRepository = userRepository;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] UserRegistrationReqDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Validation failed", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }


        //check email
        if (await _userRepository.CheckEmailExistAsync(model.Email))
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

        await _userRepository.AddUserAsync(userToAdd);

        await _userRepository.SaveChangesAsync();
       



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

        var user = await _userRepository.GetUserByEmailAsync(model.Email);

       

        if (user == null) { return Unauthorized(new { message = "User with this mail doesn't exists" }); }


        //Check if entered password is correct or not
        if (!PasswordHasher.VerifyPassword(model.Password, user.Password))
        {
            return Unauthorized(new { message = "Password is incorrect" });
        }

        user.Token = CreateJwtToken(user);
        var newAccessToken = user.Token;

        await _userRepository.SaveChangesAsync();

        

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



    [HttpGet("users")]
    [Authorize]
    public async Task<IActionResult> GetUserList()
    {
        try
        {
            //Fetching the logged in user's id from claim to filter out response
            var currentUserId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

            var users = await _userRepository.GetUsersExceptCurrentUserAsync(currentUserId);

            
            return Ok(new { users });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while retrieving the user list" });
        }
       
    }

        //private method for creating jwt token
        private string CreateJwtToken(User user)
    {
        var JwtTokenHandler = new JwtSecurityTokenHandler();
        var secret_key = _configuration["Jwt:key"];
        var key = Encoding.ASCII.GetBytes(secret_key);

        var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FullName}"),
                new Claim(ClaimTypes.Email, $"{user.Email}"),

            });

        var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = identity,
            Expires = DateTime.Now.AddDays(1),
            SigningCredentials = credentials,
            Audience = _configuration["Jwt:Audience"],
            Issuer = _configuration["Jwt:Issuer"]

        };

        var token = JwtTokenHandler.CreateToken(tokenDescriptor);

        return JwtTokenHandler.WriteToken(token);
    }

    private async Task<bool> CheckEmailExistAsync(string email)
    {
        return await _context.Users.AnyAsync(u=> u.Email == email);
    }
}


