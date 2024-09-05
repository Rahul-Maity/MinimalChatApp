using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApp.DomainModel.Dtos.Outgoing;
public class GetUserResDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
