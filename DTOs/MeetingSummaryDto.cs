using System;

namespace SWProject.ApiService.DTOs
{
    public class MeetingSummaryDto
    {
        public int MeetingId { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        public DateTime MeetingTime { get; set; }
        public int CurrentParticipants { get; set; } // 현재 참가 인원
        public int MaxParticipants { get; set; }
        public string HostUsername { get; set; }
    }
}