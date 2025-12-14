//forgot-password-script.js
const API_BASE_URL = 'http://localhost:5501/api/auth'; // 🚨 API 기본 URL
const step1 = document.getElementById('step-1');
const step2 = document.getElementById('step-2');
const step3 = document.getElementById('step-3');
const emailInput = document.getElementById('email');
const titleElement = document.getElementById('step-title');

let storedEmail = ''; // 인증 코드를 받을 때 사용한 이메일을 저장

function showStep(stepNumber) {
    step1.style.display = 'none';
    step2.style.display = 'none';
    step3.style.display = 'none';

    if (stepNumber === 1) {
        step1.style.display = 'block';
        titleElement.textContent = "비밀번호 찾기 (1/3)";
    } else if (stepNumber === 2) {
        step2.style.display = 'block';
        titleElement.textContent = "인증 코드 확인 (2/3)";
    } else if (stepNumber === 3) {
        step3.style.display = 'block';
        titleElement.textContent = "새 비밀번호 설정 (3/3)";
    }
}

// ----------------------------------------------------
// 1단계: 인증 코드 발송
// ----------------------------------------------------
document.getElementById('email-form').addEventListener('submit', async (e) => {
    e.preventDefault();
    storedEmail = emailInput.value.trim();

    try {
        const response = await fetch(`${API_BASE_URL}/send-reset-code`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email: storedEmail })
        });

        const result = await response.json();

        if (response.ok) {
            alert(result.message); // "인증 코드를 이메일로 발송했습니다."
            showStep(2); // 2단계로 이동
        } else {
            alert(result.message || '인증 코드 발송에 실패했습니다.');
        }

    } catch (error) {
        alert('서버 연결 오류가 발생했습니다.');
        console.error('Send Code Error:', error);
    }
});

// ----------------------------------------------------
// 2단계: 코드 확인
// ----------------------------------------------------
document.getElementById('code-form').addEventListener('submit', async (e) => {
    e.preventDefault();
    const token = document.getElementById('verification-code').value.trim();

    try {
        const response = await fetch(`${API_BASE_URL}/verify-code`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email: storedEmail, token: token })
        });

        const result = await response.json();

        if (response.ok) {
            alert(result.message); // "인증이 완료되었습니다."
            showStep(3); // 3단계로 이동
        } else {
            alert(result.message || '코드 확인에 실패했습니다.');
        }

    } catch (error) {
        alert('서버 연결 오류가 발생했습니다.');
        console.error('Verify Code Error:', error);
    }
});


// ----------------------------------------------------
// 3단계: 새 비밀번호 설정
// ----------------------------------------------------
document.getElementById('reset-form').addEventListener('submit', async (e) => {
    e.preventDefault();
    const newPassword = document.getElementById('new-password').value;
    const confirmPassword = document.getElementById('confirm-new-password').value;

    if (newPassword !== confirmPassword) {
        alert("새 비밀번호가 일치하지 않습니다.");
        return;
    }
    if (newPassword.length < 6) {
        alert("비밀번호는 최소 6자리 이상이어야 합니다.");
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/reset-password-final`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email: storedEmail, newPassword: newPassword })
        });

        const result = await response.json();

        if (response.ok) {
            alert(result.message); // "비밀번호가 성공적으로 재설정되었습니다."
            location.href = 'login.html'; // 로그인 페이지로 최종 이동
        } else {
            alert(result.message || '비밀번호 재설정에 실패했습니다.');
        }
    } catch (error) {
        alert('서버 연결 오류가 발생했습니다.');
        console.error('Reset Password Error:', error);
    }
});