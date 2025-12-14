using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWProject.ApiService.Models
{
    public class Meeting
    {
        [Key]
        public int Id { get; set; }

        // 외래 키: 주최자 (User)
        public int HostUserId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Location { get; set; } // 모임 장소

        [Required]
        public DateTime MeetingTime { get; set; } // 모임 일시

        public int MaxParticipants { get; set; } // 최대 참가 인원

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 관계 설정
        [ForeignKey("HostUserId")]
        public User HostUser { get; set; }

        // 모임 참가자 목록
        public ICollection<MeetingParticipant> Participants { get; set; } = new List<MeetingParticipant>();
    }
}