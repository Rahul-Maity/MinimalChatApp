using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MinimalChatApp.DomainModel.Dtos.Generic;
using MinimalChatApp.DomainModel.Dtos.Incoming;
using MinimalChatApp.DomainModel.Models;

namespace MinimalChatApp.Repository.Messages;
public interface IMessageRepository
{
    Task<bool> CheckReceiverExistsAsync(Guid receiverId);
    Task<Message> SendMessageAsync(Message message);
    Task<Message> EditMessageAsync(Guid messageId, string newContent, Guid senderId);
    Task<Message> FindMessageByIdAsync(Guid messageId);

    Task DeleteMessageAsync(Guid messageId, Guid senderId);

    Task<List<MessageDto>> RetrieveConversationAsync(Guid userId, RetrieveConversationReqDto request);

}
