namespace SWProject.ApiService.DTOs
{
    public class BoardGameSummaryDto
    {
        public int GameId { get; set; }
        public string NameKo { get; set; }
        public string Category { get; set; }
        public decimal AverageRating { get; set; } // 평균 평점
        public string ImageUrl { get; set; }
    }
}
