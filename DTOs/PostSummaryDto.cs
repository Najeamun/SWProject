namespace SWProject.ApiService.DTOs
{
    public class PostSummaryDto
    {
        public int PostId { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public string AuthorUsername { get; set; } // 작성자 아이디/닉네임
        public DateTime CreatedAt { get; set; }
        public int ViewCount { get; set; }
        public int CommentCount { get; set; }
    }
}
