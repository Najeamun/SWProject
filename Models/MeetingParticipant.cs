using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWProject.ApiService.Models
{
    public class MeetingParticipant
    {
        [Key]
        public int Id { get; set; }

        public int MeetingId { get; set; }
        public int UserId { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // 관계 설정
        [ForeignKey("MeetingId")]
        public Meeting Meeting { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}