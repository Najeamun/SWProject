namespace SWProject.ApiService.DTOs
{
    public class ProfileDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Nickname { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public string ProfileImageUrl { get; set; }
        public string BoardGamePreference { get; set; }
    }
}
