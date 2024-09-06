using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApp.DomainModel.Dtos.Incoming;
public class EditMessageReqDto
{
    [Required]
    public string Content { get; set; }
}
