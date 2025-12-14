using System.ComponentModel.DataAnnotations;

namespace SWProject.ApiService.DTOs
{
    public class CreateCommentRequest
    {
        [Required]
        public string Content { get; set; } // 👈 사용자가 입력한 댓글 내용
    }
}
