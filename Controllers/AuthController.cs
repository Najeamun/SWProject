using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // 🚨 이게 있어야 FirstOrDefaultAsync 사용 가능
using SWProject.ApiService.Data;    // 🚨 이게 있어야 AppDbContext 사용 가능
using SWProject.ApiService.DTOs;
using SWProject.ApiService.Services;

namespace SWProject.ApiService.Controllers
{

    // API 엔드포인트 경로 설정 (예: /api/auth)
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly AppDbContext _context;

        // DI를 통해 UserService 인스턴스 주입 받기
        public AuthController(UserService userService, AppDbContext context)
        {
            _userService = userService;
            _context = context;
        }
        [HttpPost("check-nickname")]
        public async Task<IActionResult> CheckNickname([FromBody] CheckNicknameRequest request)
        {
            if (string.IsNullOrEmpty(request.Nickname)) return BadRequest(new { message = "닉네임을 입력해주세요." });

            bool exists = await _userService.CheckNicknameExistsAsync(request.Nickname);
            if (exists) return Conflict(new { message = "이미 사용 중인 닉네임입니다." });

            return Ok(new { message = "사용 가능한 닉네임입니다." });
        }
        // POST 요청 처리 (회원가입 엔드포인트: POST /api/auth/register)
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var newUser = await _userService.RegisterAsync(
                    request.Username,
                    request.Email,
                    request.Password,
                    request.Nickname
                );

                return StatusCode(201, new { message = "회원가입이 성공적으로 완료되었습니다." });
            }
            catch (Exception ex)
            {
                // 🚨 에러 메시지에 따라 정확한 원인을 알려줍니다.
                if (ex.Message == "UsernameExists" || ex.Message == "EmailExists")
                {
                    // 🚨 보안을 위해 두 경우를 합쳐서 모호하게 응답
                    return Conflict(new { message = "입력하신 정보(ID 또는 이메일)가 이미 사용 중입니다." });
                }
                else if (ex.Message == "NicknameExists")
                {
                    return Conflict(new { message = "이미 사용 중인 닉네임입니다." });
                }
                else
                {
                    // 그 외 DB 오류 등
                    return StatusCode(500, new { message = $"서버 오류: {ex.Message}" });
                }
            }

        }
        // 2. 로그인 엔드포인트: POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 1. 아이디 확인
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
                return Unauthorized(new { message = "아이디 또는 비밀번호가 잘못되었습니다." });

            // 2. 비밀번호 확인 (BCrypt 검증)
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized(new { message = "아이디 또는 비밀번호가 잘못되었습니다." });

            // 3. ✅ [핵심] 로그인 성공 시 ID와 닉네임을 함께 반환!
            return Ok(new
            {
                message = "로그인 성공",
                userId = user.UserId,     // 이게 있어야 삭제/수정 권한 체크 가능
                nickname = user.Nickname, // 이게 있어야 화면에 이름 표시 가능
                token = "dummy-token"     // (나중에 JWT 쓸 자리)
            });
        }
        // DTO: EmailVerificationRequest (Email, Token), ResetPasswordRequest (Email, NewPassword)

        // 1. POST /api/auth/send-reset-code
        [HttpPost("send-reset-code")]
        public async Task<IActionResult> SendResetCode([FromBody] EmailRequest request) // EmailRequest는 Email 필드를 가진 DTO
        {
            var success = await _userService.SendResetCodeAsync(request.Email);
            if (success)
            {
                return Ok(new { message = "인증 코드를 이메일로 발송했습니다. 5분 이내에 확인해주세요." });
            }
            return NotFound(new { message = "등록된 사용자 이메일이 아닙니다." });
        }

        // 2. POST /api/auth/verify-code
        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode([FromBody] VerificationRequest request) // VerificationRequest는 Email, Token 필드를 가진 DTO
        {
            var success = await _userService.VerifyResetCodeAsync(request.Email, request.Token);
            if (success)
            {
                return Ok(new { message = "인증이 완료되었습니다. 새 비밀번호를 설정할 수 있습니다." });
            }
            return Unauthorized(new { message = "인증 코드가 유효하지 않거나 만료되었습니다." });
        }

        // 3. POST /api/auth/reset-password
        [HttpPost("reset-password-final")]
        public async Task<IActionResult> ResetPasswordFinal([FromBody] ResetPasswordRequest request)
        {
            // (여기서는 이전에 2단계가 성공했다고 가정하고 비밀번호 재설정 로직만 실행)
            var success = await _userService.ResetPasswordAsync(request.Email, request.NewPassword);

            if (success)
            {
                return Ok(new { message = "비밀번호가 성공적으로 재설정되었습니다. 다시 로그인해주세요." });
            }
            return BadRequest(new { message = "사용자를 찾을 수 없거나 재설정에 실패했습니다." });
        }
        [HttpPost("check-username")]
        public async Task<IActionResult> CheckUsername([FromBody] CheckUsernameRequest request)
        {
            if (string.IsNullOrEmpty(request.Username))
            {
                return BadRequest(new { message = "아이디를 입력해주세요." });
            }

            var exists = await _userService.CheckUsernameExistsAsync(request.Username);

            if (exists)
            {
                return Conflict(new { message = "이미 사용 중인 아이디입니다." }); // 409 Conflict 반환
            }

            return Ok(new { message = "사용 가능한 아이디입니다." }); // 200 OK 반환
        }
        // 4. GET /api/auth/profile (프로필 조회)
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            // 💡 테스트를 위해 임시 UserId를 사용합니다.
            int currentUserId = 1;

            var profile = await _userService.GetProfileAsync(currentUserId);

            if (profile == null)
            {
                return NotFound(new { message = "사용자 프로필을 찾을 수 없습니다." });
            }

            return Ok(profile);
        }

        // 5. PUT /api/auth/profile (프로필 수정)
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileDto dto)
        {
            // 💡 테스트를 위해 임시 UserId를 사용합니다.
            int currentUserId = 1;

            var success = await _userService.UpdateProfileAsync(currentUserId, dto);

            if (success)
            {
                return Ok(new { message = "프로필 정보가 성공적으로 업데이트되었습니다." });
            }

            return Conflict(new { message = "닉네임이 이미 사용 중이거나 업데이트에 실패했습니다." });
        }
    }
}

