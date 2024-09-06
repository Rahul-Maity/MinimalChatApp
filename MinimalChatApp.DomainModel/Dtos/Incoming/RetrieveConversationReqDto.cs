using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApp.DomainModel.Dtos.Incoming;
public class RetrieveConversationReqDto
{
    [Required]
    public Guid UserId { get; set; }

    public DateTime? Before { get; set; } = DateTime.UtcNow;

    [Range(1, 100)]
    public int Count { get; set; } = 20;

    [RegularExpression("^(asc|desc)$", ErrorMessage = "Sort must be 'asc' or 'desc'")]
    public string Sort { get; set; } = "asc";
}
