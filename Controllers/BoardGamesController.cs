using Microsoft.AspNetCore.Mvc;
using SWProject.ApiService.Models;
using SWProject.ApiService.Services;

namespace SWProject.ApiService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BoardGamesController : ControllerBase
    {
        private readonly BoardGameService _boardGameService;

        public BoardGamesController(BoardGameService boardGameService)
        {
            _boardGameService = boardGameService;
        }

        // 1. 전체 게임 목록 조회
        // GET: api/boardgames
        [HttpGet]
        public async Task<ActionResult<List<BoardGame>>> GetBoardGames()
        {
            return await _boardGameService.GetBoardGamesAsync();
        }

        // 2. 검색 기능
        // GET: api/boardgames/search?query=카탄
        [HttpGet("search")]
        public async Task<ActionResult<List<BoardGame>>> SearchBoardGames([FromQuery] string query, [FromQuery] string category)
        {
            // query가 없어도 카테고리만으로 검색될 수 있게 null 체크 완화
            return await _boardGameService.SearchBoardGamesAsync(query, category);
        }

        // 3. 상세 정보 조회 (리뷰 포함)
        // GET: api/boardgames/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BoardGame>> GetBoardGame(int id)
        {
            var game = await _boardGameService.GetBoardGameDetailAsync(id);
            if (game == null)
            {
                return NotFound();
            }
            return game;
        }

        // 4. 리뷰 등록
        // POST: api/boardgames/5/reviews
        [HttpPost("{id}/reviews")]
        public async Task<ActionResult<GameReview>> AddReview(int id, [FromBody] ReviewRequest request)
        {
            try
            {
                // 로그인 확인 (프론트에서 보내준 userId 사용)
                if (request.UserId <= 0)
                {
                    return BadRequest("로그인이 필요합니다.");
                }

                var review = await _boardGameService.AddReviewAsync(id, request.UserId, request.Rating, request.Content);
                return Ok(new { message = "리뷰가 등록되었습니다.", review });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"서버 오류: {ex.Message}");
            }
        }
    }

    // 리뷰 받을 때 쓸 간단한 상자 (DTO)
    public class ReviewRequest
    {
        public int UserId { get; set; }
        public int Rating { get; set; }
        public string Content { get; set; }
    }
}