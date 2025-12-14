using System.ComponentModel.DataAnnotations;

namespace SWProject.ApiService.DTOs
{
    public class ResetPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string NewPassword { get; set; }
        // 💡 실제로는 이메일 인증 코드가 추가되어야 합니다.
    }
}
