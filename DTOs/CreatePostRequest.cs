using System.ComponentModel.DataAnnotations;

namespace SWProject.ApiService.DTOs
{
    public class CreatePostRequest
    {
        [Required]
        public string Title { get; set; }
        public string Category { get; set; }
        [Required]
        public string Content { get; set; }
    }
}
