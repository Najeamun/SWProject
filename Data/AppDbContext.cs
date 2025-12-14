using Microsoft.EntityFrameworkCore;
using SWProject.ApiService.Models;

namespace SWProject.ApiService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options){}

        // DB에 생성할 테이블을 DbSet으로 정의
        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<BoardGame> BoardGames { get; set; }
        public DbSet<GameReview> GameReviews { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<MeetingParticipant> MeetingParticipants { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 🚨 EF Core의 자동 유추에 맡기고, 복잡한 설정 제거
            base.OnModelCreating(modelBuilder);
        }
    }
}
