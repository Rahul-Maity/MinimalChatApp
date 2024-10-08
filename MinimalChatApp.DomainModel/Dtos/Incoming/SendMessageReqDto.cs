﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApp.DomainModel.Dtos.Incoming;
public class SendMessageReqDto
{
    [Required]
    public Guid ReceiverId { get; set; }

    [Required]
    public string Content { get; set; }
}
