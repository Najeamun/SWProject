namespace SWProject.ApiService.DTOs
{
    public class CommentDto
    {
        public int CommentId { get; set; }
        public string Content { get; set; }
        public string AuthorUsername { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
