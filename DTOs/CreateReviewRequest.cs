using System.ComponentModel.DataAnnotations;

namespace SWProject.ApiService.DTOs
{
    public class CreateReviewRequest
    {
        [Range(1, 10)]
        public int Rating { get; set; } // 1~10점

        public string Content { get; set; }
    }
}
