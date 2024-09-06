using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApp.DomainModel.Models;
public class Message
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SenderId { get; set; }

    [ForeignKey(nameof(SenderId))]
    public User Sender { get; set; }

    [Required]
    public Guid ReceiverId { get; set; }

    [ForeignKey(nameof(ReceiverId))]
    public User Receiver { get; set; }

    [Required]
    public string Content { get; set; }

    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
