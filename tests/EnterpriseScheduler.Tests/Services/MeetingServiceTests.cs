using AutoMapper;
using EnterpriseScheduler.Exceptions;
using EnterpriseScheduler.Interfaces.Repositories;
using EnterpriseScheduler.Interfaces.Services;
using EnterpriseScheduler.Models;
using EnterpriseScheduler.Models.Common;
using EnterpriseScheduler.Models.DTOs.Meetings;
using EnterpriseScheduler.Services;

namespace EnterpriseScheduler.Tests.Services;

public class MeetingServiceTests
{
    private readonly Mock<IMeetingRepository> _meetingRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly IMeetingService _meetingService;

    public MeetingServiceTests()
    {
        _meetingRepositoryMock = new Mock<IMeetingRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _mapperMock = new Mock<IMapper>();
        _meetingService = new MeetingService(_meetingRepositoryMock.Object, _userRepositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task GetMeetingsPaginated_WithValidParameters_ReturnsPaginatedResult()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;
        var meetings = new List<Meeting>();
        var paginatedMeetings = new PaginatedResult<Meeting> { Items = meetings, TotalCount = 0 };
        var expectedResponse = new PaginatedResult<MeetingResponse> { Items = new List<MeetingResponse>(), TotalCount = 0 };

        _meetingRepositoryMock.Setup(x => x.GetPaginatedAsync(page, pageSize))
            .ReturnsAsync(paginatedMeetings);
        _mapperMock.Setup(x => x.Map<PaginatedResult<MeetingResponse>>(paginatedMeetings))
            .Returns(expectedResponse);

        // Act
        var result = await _meetingService.GetMeetingsPaginated(page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse, result);
    }

    [Fact]
    public async Task GetUserMeetings_WithValidUserId_ReturnsUserMeetings()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            TimeZone = "UTC"
        };
        var meetings = new List<Meeting>();
        var expectedResponses = new List<MeetingResponse>();

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _meetingRepositoryMock.Setup(x => x.GetUserMeetings(userId))
            .ReturnsAsync(meetings);
        _mapperMock.Setup(x => x.Map<IEnumerable<MeetingResponse>>(meetings))
            .Returns(expectedResponses);

        // Act
        var result = await _meetingService.GetUserMeetings(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponses, result);
    }

    [Fact]
    public async Task GetMeeting_WithValidId_ReturnsMeeting()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var meeting = new Meeting
        {
            Id = meetingId,
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            Participants = new List<User>()
        };
        var expectedResponse = new MeetingResponse
        {
            Id = meetingId,
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1)
        };

        _meetingRepositoryMock.Setup(x => x.GetByIdAsync(meetingId))
            .ReturnsAsync(meeting);
        _mapperMock.Setup(x => x.Map<MeetingResponse>(meeting))
            .Returns(expectedResponse);

        // Act
        var result = await _meetingService.GetMeeting(meetingId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse, result);
    }

    [Fact]
    public async Task CreateMeeting_WithValidRequest_ReturnsCreatedMeeting()
    {
        // Arrange
        var meetingRequest = new MeetingRequest
        {
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            ParticipantIds = new List<Guid> { Guid.NewGuid() }
        };

        var participants = new List<User>
        {
            new User
            {
                Id = meetingRequest.ParticipantIds.First(),
                Name = "Test User",
                TimeZone = "UTC"
            }
        };
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            Participants = new List<User>()
        };
        var expectedResponse = new MeetingResponse
        {
            Id = meeting.Id,
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1)
        };

        _userRepositoryMock.Setup(x => x.GetByIdsAsync(meetingRequest.ParticipantIds))
            .ReturnsAsync(participants);
        _meetingRepositoryMock.Setup(x => x.GetMeetingsInTimeRange(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Meeting>());
        _mapperMock.Setup(x => x.Map<Meeting>(meetingRequest))
            .Returns(meeting);
        _mapperMock.Setup(x => x.Map<MeetingResponse>(meeting))
            .Returns(expectedResponse);

        // Act
        var result = await _meetingService.CreateMeeting(meetingRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse, result);
        _meetingRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Meeting>()), Times.Once);
    }

    [Fact]
    public async Task CreateMeeting_WithTimeConflict_ThrowsMeetingConflictException()
    {
        // Arrange
        var meetingRequest = new MeetingRequest
        {
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            ParticipantIds = new List<Guid> { Guid.NewGuid() }
        };

        var participants = new List<User>
        {
            new User
            {
                Id = meetingRequest.ParticipantIds.First(),
                Name = "Test User",
                TimeZone = "UTC"
            }
        };
        var conflictingMeetings = new List<Meeting>
        {
            new Meeting
            {
                Id = Guid.NewGuid(),
                Title = "Conflicting Meeting",
                StartTime = DateTimeOffset.UtcNow.AddHours(1),
                EndTime = DateTimeOffset.UtcNow.AddHours(2),
                Participants = new List<User>()
            }
        };

        _userRepositoryMock.Setup(x => x.GetByIdsAsync(meetingRequest.ParticipantIds))
            .ReturnsAsync(participants);
        _meetingRepositoryMock.Setup(x => x.GetMeetingsInTimeRange(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(conflictingMeetings);

        // Act & Assert
        await Assert.ThrowsAsync<MeetingConflictException>(() => _meetingService.CreateMeeting(meetingRequest));
    }

    [Fact]
    public async Task UpdateMeeting_WithValidRequest_ReturnsUpdatedMeeting()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var meetingRequest = new MeetingRequest
        {
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            ParticipantIds = new List<Guid> { Guid.NewGuid() }
        };

        var existingMeeting = new Meeting
        {
            Id = meetingId,
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            Participants = new List<User>()
        };
        var participants = new List<User>
        {
            new User
            {
                Id = meetingRequest.ParticipantIds.First(),
                Name = "Test User",
                TimeZone = "UTC"
            }
           };
        var expectedResponse = new MeetingResponse
        {
            Id = meetingId,
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1)
        };

        _meetingRepositoryMock.Setup(x => x.GetByIdAsync(meetingId))
            .ReturnsAsync(existingMeeting);
        _userRepositoryMock.Setup(x => x.GetByIdsAsync(meetingRequest.ParticipantIds))
            .ReturnsAsync(participants);
        _meetingRepositoryMock.Setup(x => x.GetMeetingsInTimeRange(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Meeting>());
        _mapperMock.Setup(x => x.Map<MeetingResponse>(It.IsAny<Meeting>()))
            .Returns(expectedResponse);

        // Act
        var result = await _meetingService.UpdateMeeting(meetingId, meetingRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse, result);
        _meetingRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Meeting>()), Times.Once);
    }

    [Fact]
    public async Task DeleteMeeting_WithValidId_DeletesMeeting()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var meeting = new Meeting
        {
            Id = meetingId,
            Title = "Test Meeting",
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddHours(1),
            Participants = new List<User>()
        };

        _meetingRepositoryMock.Setup(x => x.GetByIdAsync(meetingId))
            .ReturnsAsync(meeting);

        // Act
        await _meetingService.DeleteMeeting(meetingId);

        // Assert
        _meetingRepositoryMock.Verify(x => x.DeleteAsync(meeting), Times.Once);
    }

    [Theory]
    [InlineData(0, 10)] // Invalid page
    [InlineData(1, 0)] // Invalid page size
    [InlineData(1, 101)] // Page size too large
    public async Task GetMeetingsPaginated_WithInvalidParameters_AdjustsToValidRange(int page, int pageSize)
    {
        // Arrange
        var meetings = new List<Meeting>();
        var paginatedMeetings = new PaginatedResult<Meeting>
        {
            Items = meetings,
            TotalCount = 0
        };
        var expectedResponse = new PaginatedResult<MeetingResponse>
        {
            Items = new List<MeetingResponse>(),
            TotalCount = 0
        };

        _meetingRepositoryMock.Setup(x => x.GetPaginatedAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(paginatedMeetings);
        _mapperMock.Setup(x => x.Map<PaginatedResult<MeetingResponse>>(paginatedMeetings))
            .Returns(expectedResponse);

        // Act
        var result = await _meetingService.GetMeetingsPaginated(page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse, result);
    }
}