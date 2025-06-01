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

    public async Task<IEnumerable<Meeting>> GetMeetingsForUserAsync(Guid userId, DateTime startTime, DateTime endTime)
    {
        return await _context.Meetings
            .Include(m => m.Participants)
            .Where(m => m.Participants.Any(p => p.Id == userId) &&
                       m.StartTime >= startTime &&
                       m.EndTime <= endTime)
            .ToListAsync();
    }

    public async Task<bool> HasTimeConflictAsync(Meeting meeting)
    {
        var participantIds = meeting.Participants.Select(p => p.Id).ToList();
        
        var conflictingMeetings = await _context.Meetings
            .Include(m => m.Participants)
            .Where(m => m.Participants.Any(p => participantIds.Contains(p.Id)) &&
                       m.StartTime < meeting.EndTime &&
                       m.EndTime > meeting.StartTime)
            .ToListAsync();

        return conflictingMeetings.Any();
    }

    public async Task<IEnumerable<DateTime>> FindAvailableSlotsAsync(
        IEnumerable<Guid> participantIds,
        DateTime startTime,
        DateTime endTime,
        TimeSpan duration)
    {
        var availableSlots = new List<DateTime>();
        var currentTime = startTime;

        while (currentTime + duration <= endTime)
        {
            var slotEnd = currentTime + duration;
            var hasConflict = await _context.Meetings
                .Include(m => m.Participants)
                .AnyAsync(m => m.Participants.Any(p => participantIds.Contains(p.Id)) &&
                              m.StartTime < slotEnd &&
                              m.EndTime > currentTime);

            if (!hasConflict)
            {
                availableSlots.Add(currentTime);
            }

            currentTime = currentTime.AddMinutes(30); // Check every 30 minutes
        }

        return availableSlots;
    }
} 