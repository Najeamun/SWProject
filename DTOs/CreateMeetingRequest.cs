using System;
using System.ComponentModel.DataAnnotations;

namespace SWProject.ApiService.DTOs
{
    public class CreateMeetingRequest
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        public DateTime MeetingTime { get; set; }

        [Range(2, 100)]
        public int MaxParticipants { get; set; }
        public int HostUserId { get; set; }
    }
}