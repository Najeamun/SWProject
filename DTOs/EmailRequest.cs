using System.ComponentModel.DataAnnotations;

namespace SWProject.ApiService.DTOs
{
    public class EmailRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
