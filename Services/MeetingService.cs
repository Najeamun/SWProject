using Microsoft.EntityFrameworkCore;
using SWProject.ApiService.Data;
using SWProject.ApiService.DTOs;
using SWProject.ApiService.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWProject.ApiService.Services
{
    public class MeetingService
    {
        private readonly AppDbContext _context;

        public MeetingService(AppDbContext context)
        {
            _context = context;
        }

        // 1. 모임 생성
        public async Task<bool> CreateMeetingAsync(int hostUserId, CreateMeetingRequest request)
        {
            // 호스트 사용자 확인 (FK 오류 방지)
            var hostUser = await _context.Users.FindAsync(hostUserId);
            if (hostUser == null) return false;

            var meeting = new Meeting
            {
                HostUserId = hostUserId, // FK 직접 할당
                Title = request.Title,
                Location = request.Location,
                MeetingTime = request.MeetingTime,
                MaxParticipants = request.MaxParticipants,
                CreatedAt = DateTime.UtcNow
            };

            _context.Meetings.Add(meeting);
            await _context.SaveChangesAsync();

            // 호스트를 참가자 목록에 자동 추가 (선택 사항)
            var participant = new MeetingParticipant
            {
                MeetingId = meeting.Id,
                UserId = hostUserId,
                JoinedAt = DateTime.UtcNow
            };
            _context.MeetingParticipants.Add(participant);
            await _context.SaveChangesAsync();

            return true;
        }

        // 2. 모임 목록 조회
        public async Task<List<MeetingSummaryDto>> GetMeetingsAsync()
        {
            return await _context.Meetings
                .Include(m => m.HostUser)
                .Include(m => m.Participants)
                .OrderByDescending(m => m.MeetingTime)
                .Select(m => new MeetingSummaryDto
                {
                    MeetingId = m.Id,
                    Title = m.Title,
                    Location = m.Location,
                    MeetingTime = m.MeetingTime,
                    CurrentParticipants = m.Participants.Count,
                    MaxParticipants = m.MaxParticipants,
                    HostUsername = m.HostUser.Username
                })
                .ToListAsync();
        }

        // 3. 모임 참가 신청
        public async Task<bool> JoinMeetingAsync(int meetingId, int userId)
        {
            // 이미 참가했는지 확인
            bool alreadyJoined = await _context.MeetingParticipants
                .AnyAsync(mp => mp.MeetingId == meetingId && mp.UserId == userId);

            if (alreadyJoined) return false; // 이미 참가함

            // 모임 존재 및 정원 확인
            var meeting = await _context.Meetings
                .Include(m => m.Participants)
                .FirstOrDefaultAsync(m => m.Id == meetingId);

            if (meeting == null || meeting.Participants.Count >= meeting.MaxParticipants)
            {
                return false; // 모임 없음 또는 정원 초과
            }

            // 참가 처리
            var participant = new MeetingParticipant
            {
                MeetingId = meetingId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            };

            _context.MeetingParticipants.Add(participant);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}