using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApp.DomainModel.Models;
public class ApiLog
{
    [Key]
    public int Id { get; set; }
    public string IP { get; set; }
    public string RequestBody { get; set; }
    public DateTime TimeOfCall { get; set; }
    public string Username { get; set; }
}
