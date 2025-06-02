using AutoMapper;
using EnterpriseScheduler.Constants;
using EnterpriseScheduler.Interfaces.Services;
using EnterpriseScheduler.Interfaces.Repositories;
using EnterpriseScheduler.Models;
using EnterpriseScheduler.Models.Common;
using EnterpriseScheduler.Models.DTOs.Meetings;

namespace EnterpriseScheduler.Services;

public class MeetingService : IMeetingService
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public MeetingService(IMeetingRepository meetingRepository, IUserRepository userRepository, IMapper mapper)
    {
        _meetingRepository = meetingRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<MeetingResponse>> GetMeetingsPaginated(int page, int pageSize)
    {
        if (page < PaginationConstants.MinPage) page = PaginationConstants.MinPage;
        if (pageSize < PaginationConstants.MinPageSize) pageSize = PaginationConstants.MinPageSize;
        if (pageSize > PaginationConstants.MaxPageSize) pageSize = PaginationConstants.MaxPageSize;

        var result = await _meetingRepository.GetPaginatedAsync(page, pageSize);

        return _mapper.Map<PaginatedResult<MeetingResponse>>(result);
    }

    public async Task<MeetingResponse> GetMeeting(Guid id)
    {
        var meeting = await _meetingRepository.GetByIdAsync(id);

        return _mapper.Map<MeetingResponse>(meeting);
    }

    public async Task<MeetingResponse> CreateMeeting(MeetingRequest meetingRequest)
    {
        var meeting = _mapper.Map<Meeting>(meetingRequest);
        meeting.Id = Guid.NewGuid();

        // Fetch and set participants
        var participants = await _userRepository.GetByIdsAsync(meetingRequest.ParticipantIds);
        meeting.Participants = participants.ToList();

        await _meetingRepository.AddAsync(meeting);

        return _mapper.Map<MeetingResponse>(meeting);
    }

    public async Task<MeetingResponse> UpdateMeeting(Guid id, MeetingRequest meetingRequest)
    {
        var existingMeeting = await _meetingRepository.GetByIdAsync(id);
        _mapper.Map(meetingRequest, existingMeeting);

        // Fetch and set participants
        var participants = await _userRepository.GetByIdsAsync(meetingRequest.ParticipantIds);
        existingMeeting.Participants = participants.ToList();

        await _meetingRepository.UpdateAsync(existingMeeting);

        return _mapper.Map<MeetingResponse>(existingMeeting);
    }

    public async Task DeleteMeeting(Guid id)
    {
        var meeting = await _meetingRepository.GetByIdAsync(id);
        await _meetingRepository.DeleteAsync(meeting);
    }
}