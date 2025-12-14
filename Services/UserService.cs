using BCrypt.Net;
using Microsoft.EntityFrameworkCore; // 👈 CS0029 에러 해결을 위해 필수
using SWProject.ApiService.Data;
using SWProject.ApiService.DTOs;
using SWProject.ApiService.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SWProject.ApiService.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService; // 👈 필드 추가

        public UserService(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService; // 👈 생성자 주입
        }
        
        // 비밀번호를 해시하는 단순화된 메서드 (실제 구현 시는 ASP.NET Core Identity 사용 권장)
        private string HashPassword(string password)
        {
            // 실제 구현에서는 솔트(Salt)를 사용하여 더 복잡하게 해시해야 합니다.
            // 여기서는 임시로 단순 문자열 결합 후 해시 함수를 쓴다고 가정합니다.
            //return BCrypt.Net.BCrypt.HashPassword(password);
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        // 1. 닉네임 중복 확인 메서드 추가
        public async Task<bool> CheckNicknameExistsAsync(string nickname)
        {
            return await _context.Users.AnyAsync(u => u.Nickname == nickname);
        }
        public async Task<User> RegisterAsync(string username, string email, string password, string nickname) // 👈 인자 추가
        {
            // 1. 로그인 ID (Username) 중복 체크
            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                throw new Exception("UsernameExists"); 
            }

            // 2. 이메일 중복 체크
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                throw new Exception("EmailExists");
            }

            // 3. 닉네임 중복 체크
            if (await _context.Users.AnyAsync(u => u.Nickname == nickname))
            {
                throw new Exception("NicknameExists");
            }

            // 4. 해싱 및 저장
            var passwordHash = HashPassword(password);

            var newUser = new User
            {
                Username = username,
                Email = email,
                Nickname = nickname,
                PasswordHash = passwordHash,
                Gender = "기타", // 기본값
                Age = 0,         // 기본값
                ProfileImageUrl = "",
                BoardGamePreference = ""
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return newUser;
        }
        public async Task<User> AuthenticateAsync(string username, string password)
        {
            // 1. Username으로 DB에서 사용자 검색
            var user = await _context.Users
                                     .FirstOrDefaultAsync(u => u.Username.Equals(username)); // 👈 Username으로 찾습니다.

            if (user == null)
            {
                return null; // 사용자가 존재하지 않음
            }

            // 2. 비밀번호 검증
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return null; // 비밀번호 불일치
            }

            return user;
        }
        public async Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            // 1. 이메일로 사용자 검색
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return false; // 사용자가 존재하지 않음
            }

            // 2. 새 비밀번호 해싱
            string newPasswordHash = HashPassword(newPassword);

            // 3. DB 업데이트
            user.PasswordHash = newPasswordHash;
            // user.UpdatedAt = DateTime.UtcNow; // 업데이트 시간 필드가 있다면 추가

            var result = await _context.SaveChangesAsync();

            // DB 업데이트가 성공하면 true 반환
            return result > 0;
        }
        public async Task<bool> SendResetCodeAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;

            // 1. 6자리 랜덤 코드 생성
            var token = new Random().Next(100000, 999999).ToString();
            var expiryTime = DateTime.UtcNow.AddMinutes(5); // 5분 유효

            // 2. DB에 토큰 저장
            _context.PasswordResetTokens.Add(new PasswordResetToken
            {
                Email = email,
                Token = token,
                ExpiryTime = expiryTime
            });
            await _context.SaveChangesAsync();

            // 3. 🚨 실제 이메일 발송 실행
            string subject = "비밀번호 재설정 인증 코드입니다.";
            string body = $"인증 코드: <b>{token}</b>. 5분 이내에 입력해주세요.";

            await _emailService.SendEmailAsync(email, subject, body); // 👈 실제 메일 발송!

            return true;
        }
        public async Task<bool> VerifyResetCodeAsync(string email, string token)
        {
            // 1. DB에서 가장 최근의 유효한 토큰 검색
            var validToken = await _context.PasswordResetTokens
                .Where(t => t.Email == email && t.Token == token && t.IsUsed == false)
                .OrderByDescending(t => t.ExpiryTime)
                .FirstOrDefaultAsync();

            if (validToken == null || validToken.ExpiryTime < DateTime.UtcNow)
            {
                return false; // 토큰 불일치 또는 만료
            }

            // 2. 토큰을 '사용됨'으로 표시 (재사용 방지)
            validToken.IsUsed = true;
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> CheckUsernameExistsAsync(string username)
        {
            // DB의 Users 테이블에서 해당 Username이 존재하는지 확인합니다.
            return await _context.Users.AnyAsync(u => u.Username == username);
        }
        // 1. 프로필 조회
        public async Task<ProfileDto> GetProfileAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            // User 엔티티를 DTO로 변환
            return new ProfileDto
            {
                Username = user.Username,
                Email = user.Email,
                Nickname = user.Nickname,
                Gender = user.Gender,
                Age = user.Age,
                ProfileImageUrl = user.ProfileImageUrl,
                BoardGamePreference = user.BoardGamePreference
            };
        }

        // 2. 프로필 수정
        public async Task<bool> UpdateProfileAsync(int userId, ProfileDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // 🚨 닉네임 중복 확인 (본인이 아닌 다른 사람이 사용 중인지)
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Nickname == dto.Nickname && u.UserId != userId);

            if (existingUser != null)
            {
                // 닉네임이 이미 사용 중이라면 실패 (더 복잡한 처리가 필요할 수 있음)
                return false;
            }

            // 필드 업데이트
            user.Nickname = dto.Nickname;
            user.Gender = dto.Gender;
            user.Age = dto.Age;
            user.ProfileImageUrl = dto.ProfileImageUrl;
            user.BoardGamePreference = dto.BoardGamePreference;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}