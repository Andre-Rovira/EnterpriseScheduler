using Microsoft.EntityFrameworkCore;
using EnterpriseScheduler.Data;
using EnterpriseScheduler.Models;
using EnterpriseScheduler.Interfaces.Repositories;
using EnterpriseScheduler.Models.Common;

namespace EnterpriseScheduler.Repositories;

public class MeetingRepository : IMeetingRepository
{
    private readonly ApplicationDbContext _context;

    public MeetingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<Meeting>> GetPaginatedAsync(int pageNumber, int pageSize)
    {
        var query = _context.Meetings.AsNoTracking();
        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return new PaginatedResult<Meeting>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<Meeting> GetByIdAsync(Guid id)
    {
        var meeting = await _context.Meetings.SingleOrDefaultAsync(m => m.Id == id);

        return meeting ?? throw new KeyNotFoundException($"Meeting with ID {id} not found");
    }

    public async Task AddAsync(Meeting meeting)
    {
        _context.Meetings.Add(meeting);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Meeting meeting)
    {
        _context.Meetings.Update(meeting);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Meeting meeting)
    {
        _context.Meetings.Remove(meeting);
        await _context.SaveChangesAsync();
    }
}