using System.ComponentModel.DataAnnotations;

namespace Overlord.Models;

public class Agent
{
    [Key]
    public Guid Id { get; set; }
    
    public string PersonName { get; set; }
    
    public bool IsActive { get; set; }
    public DateTime? LastCommandTime { get; set; }
}