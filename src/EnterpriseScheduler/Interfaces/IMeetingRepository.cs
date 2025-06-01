using EnterpriseScheduler.Models;

namespace EnterpriseScheduler.Interfaces;

public interface IMeetingRepository
{
    Task<Meeting> GetByIdAsync(Guid id);
    Task<IEnumerable<Meeting>> GetAllAsync();
    Task<Meeting> AddAsync(Meeting meeting);
    Task UpdateAsync(Meeting meeting);
    Task DeleteAsync(Guid id);
}