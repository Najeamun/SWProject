// edit-script.js
// 기존 게시글 로드 및 수정(PUT) 요청 처리

const API_BASE_URL = 'http://localhost:5501/api/posts'; // API 기본 경로
const editForm = document.getElementById('edit-form');
const titleInput = document.getElementById('title');
const contentInput = document.getElementById('content');
const loadingElement = document.getElementById('loading');

// URL에서 게시글 ID 추출
function getPostIdFromUrl() {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get('id');
}

// ----------------------------------------------------
// A. 기존 게시글 정보 로드 (READ)
// ----------------------------------------------------
async function loadPostForEdit() {
    const postId = getPostIdFromUrl();
    if (!postId) {
        loadingElement.textContent = "오류: 게시글 ID가 URL에 없습니다.";
        return;
    }

    try {
        // 1. 상세 보기 API 재사용하여 데이터 로드
        const response = await fetch(`${API_BASE_URL}/${postId}`);

        if (response.status === 404) {
            loadingElement.textContent = `게시글 ID ${postId}를 찾을 수 없습니다.`;
            return;
        }
        if (!response.ok) {
            throw new Error('데이터 로드 실패');
        }

        const post = await response.json();

        // 2. 폼에 기존 내용 채우기
        titleInput.value = post.title || post.Title;
        contentInput.value = post.content || post.Content;
        document.getElementById('edit-title-tag').textContent = `수정: ${post.title || post.Title}`;

        // 3. UI 전환
        loadingElement.style.display = 'none';
        editForm.style.display = 'block';

    } catch (error) {
        console.error('기존 게시글 로드 오류:', error);
        loadingElement.textContent = "기존 데이터를 불러오는 데 실패했습니다.";
    }
}

// ----------------------------------------------------
// B. 수정 완료 API 전송 함수 (PUT)
// ----------------------------------------------------
editForm.addEventListener('submit', async (e) => {
    e.preventDefault();

    const postId = getPostIdFromUrl();
    const updateData = {
        title: titleInput.value.trim(),
        content: contentInput.value.trim()
        // 카테고리 수정 기능이 있다면 여기에 추가 필요
    };

    try {
        // 1. 수정 API 호출 (PUT)
        const response = await fetch(`${API_BASE_URL}/${postId}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(updateData)
        });

        const result = await response.json();

        // 2. 응답 처리
        if (response.ok) {
            alert(`🎉 게시글이 성공적으로 수정되었습니다!`);
            location.href = `detail.html?id=${postId}`; // 상세 보기로 이동
        } else {
            alert(`수정 실패: ${result.message || '권한이 없거나 서버 오류입니다.'}`);
        }
    } catch (error) {
        console.error('수정 API 통신 오류:', error);
        alert('서버에 연결할 수 없습니다. 서버 실행 상태를 확인하세요.');
    }
});

// 페이지 로드 시 기존 내용 로드 시작
loadPostForEdit();

// 취소 버튼에서 사용할 수 있도록 전역 함수 등록
window.getPostIdFromUrl = getPostIdFromUrl;