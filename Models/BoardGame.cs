using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // 🚨 필수!

namespace SWProject.ApiService.Models
{
    public class BoardGame
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // 사용자가 ID를 지정하거나 외부 ID 사용 시
        public int Id { get; set; }

        public string NameKo { get; set; }
        public string NameEn { get; set; }
        public string Category { get; set; }
        public string CategoryDescription { get; set; }

        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public int PlayTimeMin { get; set; }

        // 난이도 (평점)
        public decimal DifficultyRating { get; set; }

        public string Designer { get; set; }
        public string ImageUrl { get; set; }
        public string ExternalLink { get; set; }

        // ✅ [추가됨] 이 게임에 달린 리뷰들 목록
        // JsonIgnore는 순환 참조 에러 방지용입니다.
        [JsonIgnore]
        public ICollection<GameReview>? Reviews { get; set; }
    }
}