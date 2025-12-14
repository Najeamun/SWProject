using Microsoft.EntityFrameworkCore;
using SWProject.ApiService.Data;
using SWProject.ApiService.DTOs;
using SWProject.ApiService.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // 👈 DbSet 관련 확장 메서드에 필요
using SWProject.ApiService.DTOs;      // 👈 CommentDto에 필요
using SWProject.ApiService.Models;     // 👈 Comment 모델에 필요

namespace SWProject.ApiService.Services
{
    public class PostService
    {
        private readonly AppDbContext _context;

        public PostService(AppDbContext context)
        {
            _context = context;
        }

        // =======================================================
        // 1. 게시글 목록 조회 (READ) - (FK 오류 우회(해결))
        // =======================================================
        // 1. 게시글 목록 조회 (카테고리 필터링 기능 추가됨)
        public async Task<List<PostSummaryDto>> GetPostsAsync(string category = "전체")
        {
            // DB에서 글을 가져올 준비
            var query = _context.Posts.Include(p => p.User).AsQueryable();

            // 🚨 [변경점] "전체"가 아니면 해당 카테고리만 골라내기
            if (!string.IsNullOrEmpty(category) && category != "전체")
            {
                query = query.Where(p => p.Category == category);
            }

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostSummaryDto
                {
                    PostId = p.Id,
                    Category = p.Category, // 🚨 [추가] DTO에 카테고리 담기
                    Title = p.Title,
                    AuthorUsername = p.User.Username,
                    CreatedAt = p.CreatedAt,
                    ViewCount = p.ViewCount,
                    CommentCount = _context.Comments.Count(c => c.PostId == p.Id)
                })
                .ToListAsync();
        }

        // 2. 게시글 작성 (카테고리 저장 기능 추가됨)
        public async Task<bool> CreatePostAsync(int userId, CreatePostRequest request)
        {
            var newPost = new Post
            {
                UserId = userId,
                Category = request.Category,
                Title = request.Title,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                ViewCount = 0
            };

            _context.Posts.Add(newPost);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
        public async Task<PostDetailDto> GetPostDetailAsync(int postId)
        {
            // 1. 게시글 조회 및 작성자 정보 포함
            var post = await _context.Posts
                .Include(p => p.User) // User 정보 조인
                .Include(p => p.Comments)   // 🚨 Comments 테이블 포함 (댓글 목록)
                .ThenInclude(c => c.User)   // 댓글 작성자 정보까지 포함
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
            {
                return null;
            }

            // 2. 조회수 증가 및 DB 저장 (비동기)
            post.ViewCount++;
            await _context.SaveChangesAsync();

            // 3. DTO로 변환하여 반환
            return new PostDetailDto
            {
                PostId = post.Id,
                Title = post.Title,
                Category = post.Category,
                Content = post.Content,
                AuthorUsername = post.User.Username,
                CreatedAt = post.CreatedAt,
                ViewCount = post.ViewCount,

                // 🚨 댓글 목록을 CommentDto로 변환하여 할당합니다.
                Comments = post.Comments.Select(c => new CommentDto
                {
                    CommentId = c.Id,
                    Content = c.Content,
                    AuthorUsername = c.User.Username,
                    CreatedAt = c.CreatedAt
                }).ToList()
            };

        }
        // =======================================================
        // 3. 게시글 삭제 
        // =======================================================
        public async Task<bool> DeletePostAsync(int postId, int userId)
        {
            // 1. 게시글을 DB에서 조회
            var post = await _context.Posts.FindAsync(postId);

            if (post == null)
            {
                return false; // 게시글이 존재하지 않음
            }

            // 2. 🚨 인증: 요청한 사용자가 해당 게시글의 작성자인지 확인
            if (post.UserId != userId)
            {
                // 작성자가 아니면 삭제 권한 없음
                return false;
            }

            // 3. 삭제 및 DB 저장
            _context.Posts.Remove(post);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }
        // 4. 게시글 수정 (Update)
        public async Task<bool> UpdatePostAsync(int postId, int userId, UpdatePostRequest request)
        {
            // 1. 게시글을 DB에서 조회
            var post = await _context.Posts.FindAsync(postId);

            if (post == null)
            {
                // 게시글이 존재하지 않으면, 실패로 처리
                return false;
            }

            // 2. 🚨 인증: 요청한 사용자가 해당 게시글의 작성자인지 확인
            // PostsController에서 전달받은 userId와 게시글에 저장된 UserId를 비교합니다.
            if (post.UserId != userId)
            {
                // 작성자가 아니면 수정 권한 없음
                return false;
            }

            // 3. 내용 업데이트
            post.Title = request.Title;
            post.Content = request.Content;
            // post.UpdatedAt = DateTime.UtcNow; // Post 모델에 UpdatedAt 필드가 있다면 추가 가능

            // 4. DB 저장
            // SaveChangesAsync를 호출하여 변경 사항을 DB에 반영합니다.
            await _context.SaveChangesAsync();

            // EF Core는 SaveChangesAsync 결과가 0이라도 (변경된 내용이 없어도) 성공으로 간주하는 것이 일반적입니다.
            return true;
        }
        // =======================================================
        // A. 댓글 목록 조회 (READ)
        // =======================================================
        public async Task<List<CommentDto>> GetCommentsAsync(int postId)
        {
            // 특정 게시글의 댓글을 조회하고 작성자 정보를 조인합니다.
            return await _context.Comments
                .Where(c => c.PostId == postId)
                .Include(c => c.User)
                .OrderBy(c => c.CreatedAt) // 오래된 댓글부터 정렬
                .Select(c => new CommentDto
                {
                    CommentId = c.Id,
                    Content = c.Content,
                    AuthorUsername = c.User.Username,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }

        // =======================================================
        // B. 댓글 작성 (CREATE)
        // =======================================================
        public async Task<bool> CreateCommentAsync(int postId, int userId, CreateCommentRequest request)
        {
            var newComment = new Comment
            {
                PostId = postId,
                UserId = userId,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(newComment);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

    }
}
