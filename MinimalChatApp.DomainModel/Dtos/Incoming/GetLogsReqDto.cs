using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApp.DomainModel.Dtos.Incoming;
public class GetLogsReqDto
{
   
    public DateTime? EndTime { get; set; } = DateTime.UtcNow;

   
    public DateTime? StartTime { get; set; } = DateTime.UtcNow.AddMinutes(-5);
}
