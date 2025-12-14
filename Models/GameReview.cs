using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization; // 필수!

namespace SWProject.ApiService.Models
{
    public class GameReview
    {
        [Key]
        public int Id { get; set; }

        // ✅ [확인] BoardGameId가 정확히 있어야 합니다.
        public int BoardGameId { get; set; }
        public int UserId { get; set; }

        public int Rating { get; set; } // 1~10점

        // 🚨 [수정됨] ReviewContent -> Content로 변경 (서비스 코드와 일치시킴)
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [JsonIgnore]
        public BoardGame? Game { get; set; }

        [JsonIgnore]
        public User? User { get; set; }
    }
}