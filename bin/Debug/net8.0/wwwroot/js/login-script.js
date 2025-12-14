// login-script.js
// 로그인 폼 제출 및 사용자 정보 저장 처리

// DOMContentLoaded를 사용해 HTML 요소가 준비되었을 때만 실행
document.addEventListener('DOMContentLoaded', () => {

    const loginForm = document.getElementById('login-form');
    // API 경로는 여기서 직접 정의하거나 별도 파일에서 가져옴
    const API_LOGIN_URL = 'http://localhost:5501/api/auth/login';

    if (!loginForm) {
        console.error("오류: login-form을 찾을 수 없습니다.");
        return;
    }

    loginForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const usernameInput = document.getElementById('username');
        const passwordInput = document.getElementById('password');

        if (!usernameInput || !passwordInput) {
            alert("입력 필드를 찾을 수 없습니다!");
            return;
        }

        const username = usernameInput.value;
        const password = passwordInput.value;

        try {
            // 1. 로그인 API 호출 (POST)
            const response = await fetch(API_LOGIN_URL, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username, password })
            });

            const result = await response.json();

            // 2. 응답 처리
            if (response.ok) {
                // 로그인 성공! 사용자 정보 (ID, 닉네임, 토큰) 저장
                localStorage.setItem('userId', result.userId);
                localStorage.setItem('nickname', result.nickname);
                localStorage.setItem('token', result.token);

                alert(result.nickname + "님 환영합니다!");
                window.location.href = 'index.html'; // 메인 페이지로 이동
            } else {
                alert("로그인 실패: " + (result.message || "오류가 발생했습니다."));
            }
        } catch (error) {
            console.error("Login error:", error);
            alert("서버 연결 실패");
        }
    });
});