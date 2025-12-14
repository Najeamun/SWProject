namespace SWProject.ApiService.DTOs
{
    public class BoardGameDetailDto
    {
        public int GameId { get; set; }
        public string NameKo { get; set; }
        public string NameEn { get; set; }
        public string Category { get; set; }
        public string CategoryDescription { get; set; }
        public string Description { get; set; } // 게임 설명
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public int PlayTimeMin { get; set; }
        public decimal DifficultyRating { get; set; }
        public string Designer { get; set; }
        public string ImageUrl { get; set; }
        public string ExternalLink { get; set; }

        // 리뷰 관련 정보
        public decimal AverageRating { get; set; }
        public List<ReviewDto> Reviews { get; set; }
    }

    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public string AuthorUsername { get; set; }
        public int Rating { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

