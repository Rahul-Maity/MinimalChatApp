using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using MinimalChatApp.DomainModel.Data;
using MinimalChatApp.DomainModel.Dtos.Generic;
using MinimalChatApp.DomainModel.Dtos.Incoming;
using MinimalChatApp.DomainModel.Dtos.Outgoing;
using MinimalChatApp.DomainModel.Models;
using MinimalChatApp.Repository.Messages;

namespace MinimalChatApp.Controllers;
[Route("api/")]
[ApiController]
public class MessagesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    private readonly IMessageRepository _messageRepository;
    public MessagesController(ApplicationDbContext context, IConfiguration configuration,IMessageRepository messageRepository)
    {
        _configuration = configuration;
        _context = context;
        _messageRepository = messageRepository;
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

        if (string.IsNullOrWhiteSpace(model.Content))
        {
            return BadRequest(new { error = "Message content cannot be empty" });
        }
        


        if (!await _messageRepository.CheckReceiverExistsAsync(model.ReceiverId))
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
            var createdMessage = await _messageRepository.SendMessageAsync(message);


           

            var response = new SendMessageResDto
            {
                MessageId = createdMessage.Id,
                SenderId = createdMessage.SenderId,
                ReceiverId = createdMessage.ReceiverId,
                Content = createdMessage.Content,
                Timestamp = createdMessage.Timestamp
            };
            return Ok(response);
        }



        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while sending the message", details = ex.Message });
        }



    }





    [HttpPut("messages/{messageId}")]
    [Authorize]
    public async Task<IActionResult> EditMessage(Guid messageId, [FromBody] EditMessageReqDto model)
    {

        // Validate the model
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { error = "Validation failed", details = errors });
        }

        var senderId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

        

        var message = await _messageRepository.FindMessageByIdAsync(messageId);
        if (message == null)
        {
            return NotFound(new { error = "Message not found" });
        }

        //check if the message sender is the one who is try to edit
        if (message.SenderId != senderId)
        {
            return Unauthorized(new { error = "You are not authorized to edit this message" });
        }

        var new_content = model.Content;
        if (string.IsNullOrWhiteSpace(new_content))
        {
            return BadRequest(new { error = "Message content cannot be empty" });
        }


        var editedMessage = await _messageRepository.EditMessageAsync(messageId, model.Content, senderId);
       



        var response = new EditMessageResDto
        {
            MessageId = editedMessage.Id,
            SenderId = editedMessage.SenderId,
            ReceiverId = editedMessage.ReceiverId,
            Content = editedMessage.Content,
            Timestamp = editedMessage.Timestamp
        };



        return Ok(response);


    }


    //endpoint for deleting message

    [HttpDelete("messages/{messageId}")]
    [Authorize]
    public async Task<IActionResult> DeleteMessage(Guid messageId)
    {

        if (messageId == Guid.Empty)
        {
            return BadRequest(new { error = "Invalid message ID" });
        }



        // Extract sender ID from the authenticated user's claims
        var senderIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(senderIdClaim) || !Guid.TryParse(senderIdClaim, out var senderId))
        {
            return Unauthorized(new { error = "Invalid authentication token" });
        }


        try
        {
            await _messageRepository.DeleteMessageAsync(messageId, senderId);
            return Ok(new { message = "Message deleted successfully" });
        }

        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Message not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { error = "You are not authorized to delete this message" });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while processing your request", details = ex.Message });
        }

        

    }


    [HttpGet("messages")]
    [Authorize]
    public async Task<IActionResult> RetrieveConversation([FromBody] RetrieveConversationReqDto request)
    {


        try
        {
            // Validate request parameters
            if (request.Count <= 0)
            {
                return BadRequest(new { error = "Count must be a positive number" });
            }

            var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            if (userId == request.UserId)
            {
                return BadRequest(new { error = "Cannot retrieve conversation with yourself" });
            }


            var messages = await _messageRepository.RetrieveConversationAsync(userId, request);

            if (messages.Count == 0)
            {
                return NotFound(new { error = "No messages found" });
            }
            return Ok(new RetrieveConversationResDto { Messages = messages });
        }
        catch(Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while processing your request", details = ex.Message });
        }



    }

}
