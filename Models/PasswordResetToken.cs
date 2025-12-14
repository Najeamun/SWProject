using System;
using System.ComponentModel.DataAnnotations;

namespace SWProject.ApiService.Models
{
    public class PasswordResetToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Email { get; set; } // 인증을 요청한 이메일

        [Required]
        public string Token { get; set; } // 서버가 발급한 6자리 인증 코드

        public DateTime ExpiryTime { get; set; } // 토큰 만료 시간

        public bool IsUsed { get; set; } = false; // 사용 여부
    }
}
