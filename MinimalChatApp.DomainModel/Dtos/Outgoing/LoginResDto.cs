using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MinimalChatApp.DomainModel.Dtos.Generic;

namespace MinimalChatApp.DomainModel.Dtos.Outgoing;
public class LoginResDto
{
    public string Token { get; set; }
    public UserProfileDto Profile { get; set; }
}
