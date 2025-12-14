using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWProject.ApiService.Models
{
    public class User
    {
        [Key] // Primary Key
        [Column("UserId")]
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; } // 암호화된 비밀번호
                                                 // 사용자 식별/커뮤니티 활동을 위한 필드
        public string Nickname { get; set; }
        // 프로필 상세 정보
        public string Gender { get; set; }
        public int Age { get; set; }
        public string ProfileImageUrl { get; set; }
        // 추천 시스템을 위한 필드
        public string BoardGamePreference { get; set; }
        public string Role { get; set; } = "USER"; // 사용자 권한
        public ICollection<Post> Posts { get; set; } = new List<Post>();


    }
}
