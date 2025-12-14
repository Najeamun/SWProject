using System.ComponentModel.DataAnnotations;

namespace SWProject.ApiService.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        public int PostId { get; set; } // 외래 키: 게시글 참조
        public int UserId { get; set; } // 외래 키: 작성자 참조

        [Required]
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Post Post { get; set; }
        public User User { get; set; }
    }
}
