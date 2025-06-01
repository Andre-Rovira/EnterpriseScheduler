using System.ComponentModel.DataAnnotations;

namespace EnterpriseScheduler.Models;

public class User
{
    public Guid Id { get; set; }
    [Required]
    public required string Name { get; set; }
    [Required]
    public required string TimeZone { get; set; }

    public ICollection<Meeting> Meetings { get; set; } = new List<Meeting>();
}