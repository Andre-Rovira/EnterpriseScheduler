using EnterpriseScheduler.Models;

namespace EnterpriseScheduler.Interfaces;

public interface IMeetingRepository
{
    Task<Meeting> GetByIdAsync(Guid id);
    Task<IEnumerable<Meeting>> GetAllAsync();
    Task<Meeting> AddAsync(Meeting meeting);
    Task UpdateAsync(Meeting meeting);
    Task DeleteAsync(Guid id);

    Task<IEnumerable<Meeting>> GetMeetingsForUserAsync(Guid userId, DateTime startTime, DateTime endTime);
    Task<bool> HasTimeConflictAsync(Meeting meeting);
    Task<IEnumerable<DateTime>> FindAvailableSlotsAsync(IEnumerable<Guid> participantIds, DateTime startTime, DateTime endTime, TimeSpan duration);
}