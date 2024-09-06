using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using MinimalChatApp.DomainModel.Data;
using MinimalChatApp.DomainModel.Dtos.Incoming;
using MinimalChatApp.DomainModel.Dtos.Outgoing;
using MinimalChatApp.DomainModel.Models;

namespace MinimalChatApp.Controllers;
[Route("api/")]
[ApiController]
public class MessagesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    public MessagesController(ApplicationDbContext context,IConfiguration configuration)
    {
        _configuration = configuration;
        _context = context;
    }

    [HttpPost("messages")]
    [Authorize]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageReqDto model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { error = "Validation failed", details = errors });
        }

        if(string.IsNullOrWhiteSpace(model.Content))
        {
            return BadRequest(new { error = "Message content cannot be empty" });
        }
        var receiverExists = await _context.Users.AnyAsync(u => u.Id == model.ReceiverId);


        if (!receiverExists)
        {
            return BadRequest(new { error = "Receiver does not exist" });
        }

        var senderId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

        if (model.ReceiverId == senderId)
        {
            return BadRequest(new { error = "You cannot send a message to yourself" });
        }

        try
        {
            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = model.ReceiverId,
                Content = model.Content,
                Timestamp = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var response = new SendMessageResDto
            {
                MessageId = message.Id,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                Content = message.Content,
                Timestamp = message.Timestamp
            };
            return Ok(response);
        }



        catch (Exception ex) {
            return StatusCode(500, new { error = "An error occurred while sending the message", details = ex.Message });
        }



    }





    [HttpPut("messages/{messageId}")]
    [Authorize]
    public async Task<IActionResult> SendMessage(Guid messageId,[FromBody] EditMessageReqDto model)
    {

        // Validate the model
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { error = "Validation failed", details = errors });
        }

        var senderId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

        var message = await _context.Messages.FindAsync(messageId);
        if (message == null)
        {
            return NotFound(new { error = "Message not found" });
        }

        //check if the message sender is the one who is try to edit
        if(message.SenderId!=senderId)
        {
            return Unauthorized(new { error = "You are not authorized to edit this message" });
        }

        var new_content = model.Content;
        if (string.IsNullOrWhiteSpace(new_content))
        {
            return BadRequest(new { error = "Message content cannot be empty" });
        }

        message.Content = new_content;

        await _context.SaveChangesAsync();



        var response = new EditMessageResDto
        {
            MessageId = message.Id,
            SenderId = message.SenderId,
            ReceiverId = message.ReceiverId,
            Content = message.Content,
            Timestamp = message.Timestamp
        };



        return Ok(response);


    }

}
