using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWProject.ApiService.Data;
using SWProject.ApiService.Models;

namespace SWProject.ApiService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeetingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MeetingsController(AppDbContext context)
        {
            _context = context;
        }

        // 1. GET: api/meetings (목록 조회)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetMeetings()
        {
            return await _context.Meetings
                .Include(m => m.HostUser) // 방장 정보
                .Include(m => m.Participants) // 참가자 정보
                .Select(m => new
                {
                    m.Id,
                    m.Title,
                    m.Location,
                    m.MeetingTime,
                    m.MaxParticipants,
                    CurrentParticipants = m.Participants.Count,
                    HostUserId = m.HostUserId,
                    HostUsername = m.HostUser.Username
                })
                .OrderByDescending(m => m.MeetingTime)
                .ToListAsync();
        }

        // 2. POST: api/meetings (생성)
        [HttpPost]
        public async Task<IActionResult> CreateMeeting([FromBody] SWProject.ApiService.DTOs.CreateMeetingRequest request)
        {
            // 1. 유효성 검사 (필수 값이 없으면 400 에러)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 2. DTO(신청서) -> Entity(DB 데이터) 변환
            var meeting = new Meeting
            {
                Title = request.Title,
                Location = request.Location,
                MeetingTime = request.MeetingTime,
                MaxParticipants = request.MaxParticipants,
                HostUserId = request.HostUserId, // 신청서에 적힌 ID 사용
                CreatedAt = DateTime.UtcNow
            };

            // 3. 모임 저장
            _context.Meetings.Add(meeting);
            await _context.SaveChangesAsync();

            // 4. 방장을 자동으로 참가자 목록에 추가
            var hostParticipant = new MeetingParticipant
            {
                MeetingId = meeting.Id,
                UserId = request.HostUserId,
                JoinedAt = DateTime.UtcNow
            };
            _context.MeetingParticipants.Add(hostParticipant);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "모임이 성공적으로 생성되었습니다.",
                meetingId = meeting.Id
            });
        }

        // 3. POST: api/meetings/{id}/join (참가 신청)
        [HttpPost("{id}/join")]
        public async Task<IActionResult> JoinMeeting(int id, [FromBody] int userId)
        {
            var meeting = await _context.Meetings
                .Include(m => m.Participants)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (meeting == null)
            {
                return NotFound(new { message = "모임을 찾을 수 없습니다." });
            }

            // 방장 체크
            if (meeting.HostUserId == userId)
            {
                return BadRequest(new { message = "모임 주최자는 이미 참가 상태입니다." });
            }

            // 중복 참가 체크
            if (meeting.Participants.Any(p => p.UserId == userId))
            {
                return Conflict(new { message = "이미 참여 중인 모임입니다." });
            }

            // 정원 초과 체크
            if (meeting.Participants.Count >= meeting.MaxParticipants)
            {
                return Conflict(new { message = "모집 정원이 마감되었습니다." });
            }

            // 참가 처리
            var participant = new MeetingParticipant
            {
                MeetingId = id,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            };

            _context.MeetingParticipants.Add(participant);
            await _context.SaveChangesAsync();

            return Ok(new { message = "참가 신청이 완료되었습니다." });
        }

        // 4. ✅ [추가됨] DELETE: api/meetings/{id} (안전한 삭제)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMeeting(int id)
        {
            // 모임과 참가자 정보를 같이 가져옴
            var meeting = await _context.Meetings
                .Include(m => m.Participants)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (meeting == null)
            {
                return NotFound(new { message = "삭제할 모임을 찾을 수 없습니다." });
            }

            try
            {
                // [안전장치] 참가자 목록(자식 데이터)을 먼저 삭제
                if (meeting.Participants != null && meeting.Participants.Any())
                {
                    _context.MeetingParticipants.RemoveRange(meeting.Participants);
                }

                // 모임(부모 데이터) 삭제
                _context.Meetings.Remove(meeting);

                await _context.SaveChangesAsync(); // 커밋

                return Ok(new { message = "모임이 안전하게 삭제되었습니다." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"삭제 중 에러 발생: {ex.Message}");
                return StatusCode(500, new { message = "삭제 중 서버 오류가 발생했습니다." });
            }
        }
    }
}