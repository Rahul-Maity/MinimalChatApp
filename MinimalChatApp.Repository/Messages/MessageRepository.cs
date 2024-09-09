using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using MinimalChatApp.DomainModel.Data;
using MinimalChatApp.DomainModel.Dtos.Generic;
using MinimalChatApp.DomainModel.Dtos.Incoming;
using MinimalChatApp.DomainModel.Models;
using MinimalChatApp.Repository.Messages;

namespace MinimalChatApp.Repository.Messages;
public class MessageRepository : IMessageRepository
{
    private readonly ApplicationDbContext _context;
    public MessageRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<bool> CheckReceiverExistsAsync(Guid receiverId)
    {
        return await _context.Users.AnyAsync(u => u.Id == receiverId);
    }

   

    public async Task<Message> EditMessageAsync(Guid messageId, string newContent, Guid senderId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null || message.SenderId != senderId)
        {
            return null;
        }
        message.Content = newContent;
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<Message> FindMessageByIdAsync(Guid messageId)
    {
        return await _context.Messages.FindAsync(messageId);
    }

    public async Task<Message> SendMessageAsync(Message message)    
    {
        await _context.Messages.AddAsync(message);
        await _context.SaveChangesAsync();
        return message;
    }



    public async Task DeleteMessageAsync(Guid messageId, Guid senderId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null)
        {
            throw new KeyNotFoundException("Message not found");
        }

        if (message.SenderId != senderId)
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this message");
        }

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();
    }

    public async Task<List<MessageDto>> RetrieveConversationAsync(Guid userId, RetrieveConversationReqDto request)
    {
        var messagesQuery = _context.Messages
            .Where(m => (m.SenderId == userId && m.ReceiverId == request.UserId) ||
                        (m.SenderId == request.UserId && m.ReceiverId == userId));

        if (request.Before.HasValue)
        {
            messagesQuery = messagesQuery.Where(m => m.Timestamp < request.Before.Value);
        }

        messagesQuery = request.Sort.ToLower() == "desc" ? messagesQuery.OrderByDescending(m => m.Timestamp)
            : messagesQuery.OrderBy(m => m.Timestamp);

        return await messagesQuery.Take(request.Count)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                ReceiverId = m.ReceiverId,
                Content = m.Content,
                Timestamp = m.Timestamp
            }).ToListAsync();
    }
}
