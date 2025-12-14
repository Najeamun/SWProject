using Microsoft.AspNetCore.Mvc;
using SWProject.ApiService.DTOs;
using SWProject.ApiService.Services;
using SWProject.ApiService.Models; // Models에 필요

namespace SWProject.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // 기본 경로: /api/posts
    public class PostsController : ControllerBase
    {
        private readonly PostService _postService;

        public PostsController(PostService postService)
        {
            _postService = postService;
        }

        // =======================================================
        // 1. 게시글 목록 조회 엔드포인트 (GET /api/posts)
        // =======================================================
        [HttpGet]
        public async Task<IActionResult> GetPosts([FromQuery] string category = "전체")
        {
            var posts = await _postService.GetPostsAsync(category);
            return Ok(posts);
        }

        // =======================================================
        // 2. 게시글 작성 엔드포인트 (POST /api/posts)
        // =======================================================
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
        {
            // 💡 중요: 현재는 테스트를 위해 임시 UserId를 사용합니다. 
            // 실제 구현 시, 로그인된 사용자의 ID를 JWT 토큰에서 추출해야 합니다.
            int tempUserId = 1;

            if (string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Content))
            {
                return BadRequest(new { message = "제목과 내용을 입력해주세요." });
            }

            var success = await _postService.CreatePostAsync(tempUserId, request);

            if (success)
            {
                // 201 Created 응답 반환
                return StatusCode(201, new { message = "게시글이 성공적으로 작성되었습니다." });
            }

            return BadRequest(new { message = "게시글 작성에 실패했습니다." });
        }
        // 3. 게시글 상세 보기 엔드포인트 (GET /api/posts/{id})
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostDetail(int id)
        {
            var postDetail = await _postService.GetPostDetailAsync(id);

            if (postDetail == null)
            {
                return NotFound(new { message = $"게시글 ID {id}를 찾을 수 없습니다." });
            }

            // 200 OK와 함께 상세 정보 반환
            return Ok(postDetail);
        }
        // 4. 게시글 삭제 엔드포인트 (DELETE /api/posts/{id})
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            // 💡 중요: 실제 사용자 ID를 토큰에서 가져와야 합니다.
            // 현재는 테스트를 위해 임시 UserId를 사용합니다.
            int currentUserId = 1;

            var success = await _postService.DeletePostAsync(id, currentUserId);

            if (success)
            {
                return Ok(new { message = "게시글이 성공적으로 삭제되었습니다." });
            }

            // 작성자가 아니거나, 게시글이 없거나 DB 오류가 발생한 경우
            return Unauthorized(new { message = "게시글 삭제 권한이 없거나 게시글을 찾을 수 없습니다." });
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, [FromBody] UpdatePostRequest request)
        {
            // 💡 테스트를 위해 임시 UserId를 사용합니다.
            // 실제 사용자 ID를 가져와야 합니다.
            int currentUserId = 1;

            if (string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Content))
            {
                return BadRequest(new { message = "제목과 내용을 모두 입력해야 합니다." });
            }

            // PostService.cs의 UpdatePostAsync 메서드를 호출합니다.
            var success = await _postService.UpdatePostAsync(id, currentUserId, request);

            if (success)
            {
                return Ok(new { message = "게시글이 성공적으로 수정되었습니다." });
            }

            // 작성자가 아니거나, 게시글을 찾을 수 없는 경우
            return Unauthorized(new { message = "수정 권한이 없거나 게시글을 찾을 수 없습니다." });
        }
        // 6. 댓글 목록 조회 (GET /api/posts/{postId}/comments)
        [HttpGet("{postId}/comments")]
        public async Task<IActionResult> GetComments(int postId)
        {
            var comments = await _postService.GetCommentsAsync(postId);
            return Ok(comments);
        }

        // 7. 댓글 작성 (POST /api/posts/{postId}/comments)
        [HttpPost("{postId}/comments")]
        public async Task<IActionResult> CreateComment(int postId, [FromBody] CreateCommentRequest request)
        {
            // 💡 테스트를 위해 임시 UserId를 사용합니다.
            int currentUserId = 1;

            if (string.IsNullOrEmpty(request.Content))
            {
                return BadRequest(new { message = "댓글 내용을 입력해주세요." });
            }

            var success = await _postService.CreateCommentAsync(postId, currentUserId, request);

            if (success)
            {
                return StatusCode(201, new { message = "댓글이 작성되었습니다." });
            }

            return BadRequest(new { message = "게시글을 찾을 수 없거나 댓글 작성에 실패했습니다." });
        }
    }
}
