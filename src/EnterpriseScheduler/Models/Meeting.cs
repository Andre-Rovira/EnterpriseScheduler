using System.ComponentModel.DataAnnotations;

namespace EnterpriseScheduler.Models;

public class Meeting
{
    public Guid Id { get; set; }
    [Required]
    public required string Title { get; set; }
    [Required]
    public DateTimeOffset StartTime { get; set; }
    [Required]
    public DateTimeOffset EndTime { get; set; }

    public ICollection<User> Participants { get; set; } = new List<User>();
}