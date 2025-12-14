using System.ComponentModel.DataAnnotations;

namespace SWProject.ApiService.DTOs
{
    // 비밀번호 찾기 두 번째 단계에서 이메일과 토큰을 요청할 때 사용
    public class VerificationRequest
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; } // 클라이언트가 입력한 인증 코드
    }
}
