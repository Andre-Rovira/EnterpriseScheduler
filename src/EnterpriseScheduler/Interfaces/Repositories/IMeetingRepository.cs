using EnterpriseScheduler.Models;
using EnterpriseScheduler.Models.Common;

namespace EnterpriseScheduler.Interfaces.Repositories;

public interface IMeetingRepository
{
    Task<PaginatedResult<Meeting>> GetPaginatedAsync(int pageNumber, int pageSize);
    Task<Meeting> GetByIdAsync(Guid id);
    Task AddAsync(Meeting meeting);
    Task UpdateAsync(Meeting meeting);
    Task DeleteAsync(Meeting meeting);
}
