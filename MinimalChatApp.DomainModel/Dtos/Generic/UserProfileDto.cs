using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApp.DomainModel.Dtos.Generic;
public class UserProfileDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
