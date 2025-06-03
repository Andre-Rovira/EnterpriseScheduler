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
        ConvertToUtc(meetingRequest);
        ValidateMeetingTimes(meetingRequest);

        var meeting = _mapper.Map<Meeting>(meetingRequest);
        meeting.Id = Guid.NewGuid();

        meeting.Participants = await ValidateAndGetParticipants(meetingRequest.ParticipantIds);

        await _meetingRepository.AddAsync(meeting);

        return _mapper.Map<MeetingResponse>(meeting);
    }

    public async Task<MeetingResponse> UpdateMeeting(Guid id, MeetingRequest meetingRequest)
    {
        ConvertToUtc(meetingRequest);
        ValidateMeetingTimes(meetingRequest);

        var existingMeeting = await _meetingRepository.GetByIdAsync(id);
        _mapper.Map(meetingRequest, existingMeeting);

        existingMeeting.Participants = await ValidateAndGetParticipants(meetingRequest.ParticipantIds);

        await _meetingRepository.UpdateAsync(existingMeeting);

        return _mapper.Map<MeetingResponse>(existingMeeting);
    }

    public async Task DeleteMeeting(Guid id)
    {
        var meeting = await _meetingRepository.GetByIdAsync(id);
        await _meetingRepository.DeleteAsync(meeting);
    }

    private void ConvertToUtc(MeetingRequest request)
    {
        request.StartTime = request.StartTime.ToUniversalTime();
        request.EndTime = request.EndTime.ToUniversalTime();
    }

    private void ValidateMeetingTimes(MeetingRequest meetingRequest)
    {
        if (meetingRequest.StartTime >= meetingRequest.EndTime)
        {
            throw new ArgumentException("Start time must be before end time.");
        }
    }

    private async Task<List<User>> ValidateAndGetParticipants(IEnumerable<Guid> participantIds)
    {
        if (participantIds == null || !participantIds.Any())
        {
            throw new ArgumentException("At least one participant is required for a meeting.");
        }

        var participants = await _userRepository.GetByIdsAsync(participantIds);
        if (!participants.Any())
        {
            throw new ArgumentException("At least one valid participant ID is required.");
        }

        return participants.ToList();
    }
}
