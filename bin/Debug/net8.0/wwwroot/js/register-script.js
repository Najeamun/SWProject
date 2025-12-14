// register-script.js
// 회원가입 폼 유효성 검사 및 등록(POST) 요청 처리

// API 경로 정의
const CHECK_NICKNAME_API = 'http://localhost:5501/api/auth/check-nickname';
const REGISTER_API_URL = 'http://localhost:5501/api/auth/register';

// DOM 요소
const usernameInput = document.getElementById('username');
const nicknameInput = document.getElementById('nickname');
const checkNicknameBtn = document.getElementById('check-nickname');
const nicknameStatus = document.getElementById('nickname-status');
const passwordInput = document.getElementById('password');
const confirmPasswordInput = document.getElementById('confirm-password');
const submitBtn = document.getElementById('submit-btn');
const passwordMatchStatus = document.getElementById('password-match-status');
const emailInput = document.getElementById('email');

// 상태 변수
let isNicknameChecked = false; // 닉네임 중복 확인 여부
let isPasswordMatched = false; // 비밀번호 일치 및 길이 충족 여부

// ----------------------------------------------------
// 1. 버튼 활성화 상태 업데이트
// ----------------------------------------------------
function updateSubmitButton() {
    // 비밀번호 일치 및 길이 (6자 이상) 체크
    isPasswordMatched = (passwordInput.value.length >= 6) && (passwordInput.value === confirmPasswordInput.value);

    // 기타 필수 조건 체크
    const isUsernameOk = usernameInput.value.trim().length >= 4;
    const isEmailOk = emailInput.value.trim().length > 0;

    // 최종 조건: 닉네임 중복 확인됨 + 비밀번호 일치/길이 OK + 아이디 길이 OK + 이메일 있음
    if (isNicknameChecked && isPasswordMatched && isUsernameOk && isEmailOk) {
        submitBtn.disabled = false;
        submitBtn.style.backgroundColor = "#E67E22"; // 활성화 색상
    } else {
        submitBtn.disabled = true;
        submitBtn.style.backgroundColor = "#ccc"; // 비활성화 색상
    }
}

// ----------------------------------------------------
// 2. 닉네임 중복 확인 (API POST 요청)
// ----------------------------------------------------
checkNicknameBtn.addEventListener('click', async () => {
    const nickname = nicknameInput.value.trim();
    if (!nickname) {
        alert('닉네임을 입력해주세요.');
        return;
    }

    try {
        const response = await fetch(CHECK_NICKNAME_API, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ nickname: nickname })
        });

        const result = await response.json();

        if (response.ok) { // 200 OK
            nicknameStatus.textContent = "사용 가능한 닉네임입니다.";
            nicknameStatus.style.color = "green";
            isNicknameChecked = true;
        } else { // 409 Conflict 등
            nicknameStatus.textContent = "이미 사용 중인 닉네임입니다.";
            nicknameStatus.style.color = "red";
            isNicknameChecked = false;
        }
    } catch (e) {
        console.error(e);
        nicknameStatus.textContent = "서버 통신 오류.";
        nicknameStatus.style.color = "red";
    }
    updateSubmitButton();
});

// ----------------------------------------------------
// 3. 입력값 변경 감지 및 상태 초기화/갱신
// ----------------------------------------------------

// 닉네임 수정 시 중복 확인 상태 초기화
nicknameInput.addEventListener('input', () => {
    isNicknameChecked = false;
    nicknameStatus.textContent = "";
    updateSubmitButton();
});

// 나머지 필드 변경 시 버튼 활성화 상태만 업데이트
usernameInput.addEventListener('input', updateSubmitButton);
emailInput.addEventListener('input', updateSubmitButton);
passwordInput.addEventListener('input', updateSubmitButton);


// 비밀번호 일치 확인 로직 (실시간 감지)
confirmPasswordInput.addEventListener('input', () => {
    const pw = passwordInput.value;
    const confirmPw = confirmPasswordInput.value;

    if (pw === confirmPw && pw.length >= 6) {
        if (passwordMatchStatus) { passwordMatchStatus.textContent = "비밀번호 일치"; passwordMatchStatus.style.color = "green"; }
    } else {
        if (passwordMatchStatus) { passwordMatchStatus.textContent = "비밀번호 불일치 또는 너무 짧음 (6자 이상)"; passwordMatchStatus.style.color = "red"; }
    }
    updateSubmitButton();
});


// ----------------------------------------------------
// 4. 회원가입 요청 전송 (FINAL POST)
// ----------------------------------------------------
document.getElementById('signup-form').addEventListener('submit', async (e) => {
    e.preventDefault();

    if (!isNicknameChecked) {
        alert("닉네임 중복 확인을 해주세요.");
        return;
    }
    if (!isPasswordMatched) {
        alert("비밀번호를 6자 이상 입력하고 일치하는지 확인해주세요.");
        return;
    }

    const userData = {
        username: usernameInput.value.trim(),
        nickname: nicknameInput.value.trim(),
        email: emailInput.value.trim(),
        password: passwordInput.value
    };

    try {
        const response = await fetch(REGISTER_API_URL, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(userData)
        });

        if (response.ok) {
            alert("회원가입 성공! 로그인 페이지로 이동합니다.");
            window.location.href = 'login.html';
        } else {
            const error = await response.json();
            alert("가입 실패: " + (error.message || "알 수 없는 서버 오류"));
        }
    } catch (e) {
        console.error(e);
        alert("서버 통신 오류");
    }
});

// 초기 버튼 상태 설정
updateSubmitButton();