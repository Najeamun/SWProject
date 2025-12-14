namespace SWProject.ApiService.DTOs
{
    public class UpdatePostRequest
    {
        // 사용자가 수정을 요청할 때 전달하는 제목
        public string Title { get; set; }

        // 사용자가 수정을 요청할 때 전달하는 내용
        public string Content { get; set; }
    }
}
