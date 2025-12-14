namespace SWProject.ApiService.DTOs
{
    public class PostDetailDto
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Content { get; set; } // 상세 보기이므로 내용 전체 포함
        public string AuthorUsername { get; set; } // 작성자 아이디
        public DateTime CreatedAt { get; set; }
        public int ViewCount { get; set; }
        // 🚨 이 필드를 추가하여 댓글 목록을 통합합니다.
        public List<CommentDto> Comments { get; set; } 
    }
}
