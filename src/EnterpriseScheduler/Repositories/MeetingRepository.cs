using Microsoft.EntityFrameworkCore;
using EnterpriseScheduler.Data;
using EnterpriseScheduler.Models;
using EnterpriseScheduler.Interfaces;

namespace EnterpriseScheduler.Repositories;

public class MeetingRepository : IMeetingRepository
{
    private readonly ApplicationDbContext _context;

    public MeetingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Meeting> GetByIdAsync(Guid id)
    {
        var meeting = await _context.Meetings.SingleOrDefaultAsync(m => m.Id == id);
        
        return meeting ?? throw new KeyNotFoundException($"Meeting with ID {id} not found");
    }

    public async Task<IEnumerable<Meeting>> GetAllAsync()
    {
        return await _context.Meetings.ToListAsync();
    }

    public async Task<Meeting> AddAsync(Meeting meeting)
    {
        await _context.Meetings.AddAsync(meeting);
        await _context.SaveChangesAsync();
        return meeting;
    }

    public async Task UpdateAsync(Meeting meeting)
    {
        _context.Meetings.Update(meeting);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var meeting = await _context.Meetings.FindAsync(id);
        if (meeting != null)
        {
            _context.Meetings.Remove(meeting);
            await _context.SaveChangesAsync();
        }
    }
} 