// profile-script.js
// 프로필 정보 로드 및 수정(PUT) 요청 처리

const API_PROFILE_URL = 'http://localhost:5501/api/auth/profile'; // API 기본 경로
const profileForm = document.getElementById('profile-form');
const usernameInput = document.getElementById('username');
const nicknameInput = document.getElementById('nickname');
const ageInput = document.getElementById('age');
const genderSelect = document.getElementById('gender');
const preferenceInput = document.getElementById('preference');
const loadingElement = document.getElementById('loading');

// ----------------------------------------------------
// A. 기존 프로필 정보 로드 (READ)
// ----------------------------------------------------
async function loadProfile() {
    loadingElement.style.display = 'block';

    try {
        // 💡 실제 구현 시, 토큰을 헤더에 포함하여 현재 사용자의 프로필을 요청해야 합니다.
        const response = await fetch(API_PROFILE_URL);

        if (response.status === 404) {
            loadingElement.textContent = "프로필을 찾을 수 없습니다.";
            return;
        }
        if (!response.ok) {
            throw new Error('프로필 로드 실패');
        }

        const profile = await response.json();

        // 폼에 기존 내용 채우기
        usernameInput.value = profile.username;
        nicknameInput.value = profile.nickname || '';
        ageInput.value = profile.age || '';
        genderSelect.value = profile.gender || '';
        preferenceInput.value = profile.boardGamePreference || '';

        // UI 전환
        loadingElement.style.display = 'none';
        profileForm.style.display = 'block';

    } catch (error) {
        console.error('프로필 로드 오류:', error);
        loadingElement.textContent = "데이터 로드에 실패했습니다. 서버를 확인하세요.";
    }
}

// ----------------------------------------------------
// B. 프로필 수정 완료 API 전송 (PUT)
// ----------------------------------------------------
profileForm.addEventListener('submit', async (e) => {
    e.preventDefault();

    const updateData = {
        // 서버 DTO에 맞춰 필드명 사용
        username: usernameInput.value,
        email: 'test@example.com', // 💡 실제로는 사용자가 입력해야 함
        nickname: nicknameInput.value,
        gender: genderSelect.value,
        age: parseInt(ageInput.value || 0),
        profileImageUrl: 'default.jpg', // 임시 값
        boardGamePreference: preferenceInput.value
    };

    try {
        const response = await fetch(API_PROFILE_URL, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(updateData)
        });

        const result = await response.json();

        if (response.ok) {
            alert(`🎉 ${result.message || '프로필이 성공적으로 수정되었습니다.'}`);
            // 수정 후 닉네임이 바뀌었다면 localStorage 갱신 필요
            localStorage.setItem('nickname', updateData.nickname);
            loadProfile(); // 최신 정보 다시 로드
        } else {
            alert(`수정 실패: ${result.message || '서버 오류'}`);
        }
    } catch (error) {
        alert('API 통신 오류가 발생했습니다.');
        console.error('Update Profile Error:', error);
    }
});

loadProfile();