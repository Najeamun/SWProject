using System.ComponentModel.DataAnnotations;

namespace SWProject.ApiService.DTOs
{
    public class RegisterRequest
    {
        [Required]
        [MinLength(4)]
        public string Username { get; set; } // 아이디

        [Required]
        public string Nickname { get; set; } // 활동명
        [Required]

        [EmailAddress]
        public string Email { get; set; } // 이메일

        [Required]
        [MinLength(6)]
        public string Password { get; set; } // 비밀번호
    }
}
