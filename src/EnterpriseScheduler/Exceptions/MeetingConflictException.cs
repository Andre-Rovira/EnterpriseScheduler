using EnterpriseScheduler.Models;

namespace EnterpriseScheduler.Exceptions;

public class MeetingConflictException : Exception
{
    public IEnumerable<TimeSlot> AvailableSlots { get; }

    public MeetingConflictException(IEnumerable<TimeSlot> availableSlots) 
        : base($"Conflicts with existing meetings. Here are the next available slots: {string.Join(", ", availableSlots)}")
    {
        AvailableSlots = availableSlots;
    }
} 