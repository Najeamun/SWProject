// js/meetings-script.js

const API_MEETINGS_URL = 'http://localhost:5501/api/meetings';

document.addEventListener('DOMContentLoaded', () => {

    // 0. 로그인 상태 UI (우측 상단 닉네임 표시)
    const nickname = localStorage.getItem('nickname');
    const profileArea = document.getElementById('user-profile');
    if (nickname && profileArea) {
        profileArea.innerHTML = `
            <span style="color: white; margin-right:10px; font-weight: bold;">👋 ${nickname}님</span>
            <a href="#" onclick="logout()" style="color:#ffcccc; text-decoration:underline; font-size:0.9em; cursor: pointer;">로그아웃</a>
        `;
    }

    // 요소 가져오기
    const toggleBtn = document.getElementById('toggle-btn');
    const createFormContainer = document.getElementById('create-form-container');
    const createForm = document.getElementById('create-meeting-form');
    // 🚨 중요: meetings.html에서 테이블 body에 id="meeting-list-body"를 주었는지 확인하세요.
    const tableBody = document.getElementById('meeting-list-body');

    // 1. [토글 버튼] 모임 만들기 폼 열기/닫기
    if (toggleBtn) {
        toggleBtn.addEventListener('click', () => {
            // 폼이 숨겨져 있으면 보이기
            if (createFormContainer.style.display === 'none' || createFormContainer.style.display === '') {
                createFormContainer.style.display = 'block';
                toggleBtn.textContent = "취소";
                toggleBtn.className = "btn btn-accent"; // 오렌지색(취소 느낌)
            } else {
                // 폼이 보이면 숨기기
                createFormContainer.style.display = 'none';
                toggleBtn.textContent = "+ 모임 만들기";
                toggleBtn.className = "btn btn-primary"; // 네이비색(기본)
            }
        });
    }

    // 2. [모임 생성] 폼 제출 처리
    if (createForm) {
        createForm.addEventListener('submit', async (e) => {
            e.preventDefault();

            // 로그인 체크
            const userId = localStorage.getItem('userId');
            if (!userId) {
                alert("로그인이 필요한 서비스입니다.");
                return;
            }

            // 데이터 준비 (DTO 형식에 맞춤)
            const meetingData = {
                title: document.getElementById('title').value,
                location: document.getElementById('location').value,
                meetingTime: document.getElementById('meeting-time').value,
                maxParticipants: parseInt(document.getElementById('max-participants').value),
                hostUserId: parseInt(userId) // 방장 ID (숫자 변환 필수)
            };

            try {
                const response = await fetch(API_MEETINGS_URL, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(meetingData)
                });

                if (response.status === 201) {
                    alert('🎉 모임이 성공적으로 생성되었습니다!');
                    createForm.reset(); // 입력창 초기화
                    createFormContainer.style.display = 'none'; // 폼 닫기
                    toggleBtn.textContent = "+ 모임 만들기";
                    toggleBtn.className = "btn btn-primary";
                    fetchMeetings(); // 목록 새로고침
                } else {
                    const errorData = await response.json();
                    alert('생성 실패: ' + (errorData.message || "서버 오류"));
                }
            } catch (error) {
                console.error(error);
                alert("서버 연결 실패");
            }
        });
    }

    // 3. [목록 조회] 서버에서 모임 리스트 가져오기
    async function fetchMeetings() {
        try {
            const response = await fetch(API_MEETINGS_URL);

            if (!response.ok) {
                throw new Error('네트워크 응답 오류');
            }

            const meetings = await response.json();
            renderMeetings(meetings);

        } catch (error) {
            console.error("목록 로드 실패:", error);
            if (tableBody) {
                tableBody.innerHTML = '<tr><td colspan="5">데이터를 불러오지 못했습니다. 서버 상태를 확인해주세요.</td></tr>';
            }
        }
    }

    // 4. [화면 그리기] 가져온 데이터를 테이블에 출력
    function renderMeetings(meetings) {
        if (!tableBody) return; // 테이블이 없으면 중단
        tableBody.innerHTML = '';

        if (meetings.length === 0) {
            tableBody.innerHTML = '<tr><td colspan="5">현재 모집 중인 모임이 없습니다.</td></tr>';
            return;
        }

        const currentUserId = parseInt(localStorage.getItem('userId') || 0);

        meetings.forEach(meeting => {
            const row = document.createElement('tr');

            // 🚨 ID 찾기 (서버 DTO 필드명 대소문자 차이 방지)
            const realId = meeting.id || meeting.meetingId || meeting.Id;

            // 날짜 예쁘게 변환
            const dateObj = new Date(meeting.meetingTime);
            const dateStr = `${dateObj.getMonth() + 1}/${dateObj.getDate()} ${dateObj.getHours()}:${String(dateObj.getMinutes()).padStart(2, '0')}`;

            // 상태 체크
            const isFull = meeting.currentParticipants >= meeting.maxParticipants;
            const isHost = meeting.hostUserId === currentUserId;

            // 뱃지 (모집중/마감)
            let statusBadge = isFull
                ? `<span class="badge-close">마감</span>`
                : `<span class="badge-open">모집중</span>`;

            // 버튼 결정 (참가/삭제/마감)
            let actionBtn = '';

            if (isHost) {
                // 내가 만든 모임 -> 삭제 버튼
                actionBtn = `<button onclick="deleteMeeting(${realId})" style="padding:5px 10px; background-color:#c0392b; color:white; border:none; border-radius:4px; cursor:pointer; font-size:0.85em;">삭제</button>`;
            } else if (isFull) {
                // 꽉 찬 모임 -> 비활성화 버튼
                actionBtn = `<button disabled style="padding:5px 10px; background-color:#ccc; color:white; border:none; border-radius:4px; font-size:0.85em;">마감</button>`;
            } else {
                // 참가 가능 -> 신청 버튼
                actionBtn = `<button onclick="joinMeeting(${realId})" style="padding:5px 10px; background-color:#27ae60; color:white; border:none; border-radius:4px; cursor:pointer; font-size:0.85em;">신청 (${meeting.currentParticipants}/${meeting.maxParticipants})</button>`;
            }

            // 행(Row) 내용 채우기
            row.innerHTML = `
                <td>${statusBadge}</td>
                <td class="title-cell">
                    <span style="font-weight:bold; font-size:1.05em;">${meeting.title}</span>
                    <br>
                    <span style="font-size:0.85em; color:#666;">📍 ${meeting.location}</span>
                </td>
                <td>${meeting.hostUsername || '익명'}</td>
                <td>${dateStr}</td>
                <td>${actionBtn}</td>
            `;

            tableBody.appendChild(row);
        });
    }

    // 페이지 로드 시 목록 불러오기 실행
    fetchMeetings();

}); // 🚨 여기가 중요합니다! (DOMContentLoaded 닫는 괄호)


// ============================================================
// 5. 전역 함수 (HTML의 onclick에서 호출하기 위해 window에 등록)
// ============================================================

// [참가 신청 함수]
window.joinMeeting = async function (meetingId) {
    const userId = localStorage.getItem('userId');
    if (!userId) {
        alert("로그인이 필요합니다.");
        window.location.href = '../../pages/auth/login.html';
        return;
    }

    if (!confirm("이 모임에 참가하시겠습니까?")) return;

    try {
        const res = await fetch(`${API_MEETINGS_URL}/${meetingId}/join`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(parseInt(userId))
        });

        const result = await res.json();

        if (res.ok) {
            alert("✅ " + result.message);
            location.reload();
        } else {
            alert("✋ " + (result.message || "신청 실패"));
        }
    } catch (e) {
        console.error(e);
        alert("서버 통신 오류");
    }
};

// [모임 삭제 함수]
window.deleteMeeting = async function (meetingId) {
    if (!confirm("정말로 이 모임을 삭제하시겠습니까? (복구 불가)")) return;

    try {
        const res = await fetch(`${API_MEETINGS_URL}/${meetingId}`, {
            method: 'DELETE'
        });

        if (res.ok) {
            alert("🗑️ 모임이 삭제되었습니다.");
            location.reload();
        } else {
            const errorText = await res.text();
            console.error(errorText);
            alert("삭제 실패: 권한이 없거나 이미 삭제된 모임입니다.");
        }
    } catch (e) {
        console.error(e);
        alert("서버 통신 오류");
    }
};

// [로그아웃 함수]
window.logout = function () {
    localStorage.clear();
    alert("로그아웃 되었습니다.");
    window.location.href = '../../pages/auth/login.html';
};