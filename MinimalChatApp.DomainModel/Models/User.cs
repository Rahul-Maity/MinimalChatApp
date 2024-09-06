using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApp.DomainModel.Models;
public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string FullName { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    public string Token {  get; set; }

    // Navigation properties
    public ICollection<Message> SentMessages { get; set; } = new List<Message>(); 
    public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
}
