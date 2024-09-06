using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MinimalChatApp.DomainModel.Dtos.Generic;

namespace MinimalChatApp.DomainModel.Dtos.Outgoing;
public class RetrieveConversationResDto
{
    public List<MessageDto> Messages { get; set; } = new List<MessageDto>();
}
    