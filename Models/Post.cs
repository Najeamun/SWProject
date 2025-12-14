using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SWProject.ApiService.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; } // PK는 'Id'로 단순화

        // 외래 키 (FK) 컬럼
        public int UserId { get; set; }
        public string Category { get; set; } = "전체";
        public string Title { get; set; }
        public string Content { get; set; }
        public int ViewCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property (FK 관계 설정을 위해 필요)
        public User User { get; set; }
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
