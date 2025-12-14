using System.ComponentModel.DataAnnotations;

namespace SWProject.ApiService.DTOs
{
    public class LoginRequest
    {
        [Required]
        [MinLength(4)] // 최소 길이 제약 조건 유지 (선택적)
        public string Username { get; set; } // 👈 이제 일반 ID를 받습니다.

        [Required]
        public string Password { get; set; }
    }
}
