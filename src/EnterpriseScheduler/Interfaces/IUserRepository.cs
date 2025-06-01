using EnterpriseScheduler.Models;

namespace EnterpriseScheduler.Interfaces;

public interface IUserRepository
{
    Task<PaginatedResult<User>> GetPaginatedAsync(int pageNumber, int pageSize);
    Task<User> GetByIdAsync(Guid id);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
}