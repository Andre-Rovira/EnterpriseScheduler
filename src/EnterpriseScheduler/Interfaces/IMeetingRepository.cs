using EnterpriseScheduler.Models;

namespace EnterpriseScheduler.Interfaces;

public interface IMeetingRepository
{
    Task<PaginatedResult<Meeting>> GetPaginatedAsync(int pageNumber, int pageSize);
    Task<Meeting> GetByIdAsync(Guid id);
    Task AddAsync(Meeting meeting);
    Task UpdateAsync(Meeting meeting);
    Task DeleteAsync(Meeting meeting);
}