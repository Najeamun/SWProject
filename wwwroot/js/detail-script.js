// detail-script.js
// 게시글 상세, 댓글 조회 및 등록, 삭제 기능 담당

const API_URL = 'http://localhost:5501/api/posts'; // API 기본 경로

// URL에서 게시글 ID 추출
function getPostId() {
    const params = new URLSearchParams(window.location.search);
    return params.get('id');
}

// ----------------------------------------------------
// 1. 게시글 상세 정보 가져오기 (READ)
// ----------------------------------------------------
async function fetchPostDetail() {
    const postId = getPostId();
    if (!postId) {
        alert("잘못된 접근입니다.");
        location.href = 'board.html';
        return;
    }

    try {
        const response = await fetch(`${API_URL}/${postId}`);

        if (!response.ok) {
            throw new Error("글을 찾을 수 없거나 서버 오류");
        }

        const post = await response.json();
        renderPost(post);
    } catch (error) {
        console.error("데이터 로드 에러:", error);
        alert("글을 불러오는데 실패했습니다. (삭제되었거나 서버 오류)");
        location.href = 'board.html'; // 목록으로 이동
    }
}

// ----------------------------------------------------
// 2. 화면 렌더링 (DOM 업데이트)
// ----------------------------------------------------
function renderPost(post) {
    // 🚨 DTO 필드명 대소문자 방어 로직 적용

    // 상단 정보 영역
    document.getElementById('post-category').textContent = `[${post.category || post.Category || '미분류'}]`;
    document.getElementById('post-title').textContent = post.title || post.Title;
    document.getElementById('post-author').textContent = post.authorUsername || post.AuthorUsername || '익명';

    const dateRaw = post.createdAt || post.CreatedAt;
    document.getElementById('post-created-at').textContent = dateRaw ? new Date(dateRaw).toLocaleDateString() : '-';
    document.getElementById('post-views').textContent = post.viewCount || post.ViewCount || 0;

    // 본문 내용 (개행 문자를 위해 textContent 사용)
    document.getElementById('post-content').textContent = post.content || post.Content || "내용이 없습니다.";

    // 댓글 수 업데이트
    const comments = post.comments || post.Comments || [];
    document.getElementById('comment-count').textContent = comments.length;

    // 본인 글일 때만 수정/삭제 버튼 보이기 (현재는 임시로 무조건 표시)
    document.getElementById('delete-button').style.display = 'inline-block';
    document.getElementById('edit-button').style.display = 'inline-block';

    // 댓글 렌더링 호출
    renderComments(comments);

    // 🚨 [핵심] 로딩 화면 해제 및 상세 내용 표시
    const loadingElement = document.getElementById('loading');
    const detailElement = document.getElementById('post-detail');
    if (loadingElement) loadingElement.style.display = 'none';
    if (detailElement) detailElement.style.display = 'block';
}

// ----------------------------------------------------
// 3. 댓글 목록 렌더링
// ----------------------------------------------------
function renderComments(comments) {
    const list = document.getElementById('comments-list');
    if (!list) return;

    list.innerHTML = '';

    if (!comments || comments.length === 0) {
        list.innerHTML = '<li style="color:#999; text-align:center; padding:10px;">작성된 댓글이 없습니다.</li>';
        return;
    }

    comments.forEach(c => {
        const li = document.createElement('li');

        // 댓글 데이터도 DTO 대소문자 방어
        const author = c.authorUsername || c.AuthorUsername || '익명';
        const content = c.content || c.Content || '';
        const dateRaw = c.createdAt || c.CreatedAt;
        const date = dateRaw ? new Date(dateRaw).toLocaleDateString() : '';

        li.innerHTML = `
            <div style="display:flex; justify-content:space-between; margin-bottom:5px; border-bottom:1px solid #eee; padding-bottom:5px;">
                <strong>${author}</strong>
                <span style="font-size:0.8em; color:#888;">${date}</span>
            </div>
            <div style="white-space: pre-wrap;">${content}</div>
        `;
        li.style.padding = "10px";
        li.style.borderBottom = "1px solid #f0f0f0";

        list.appendChild(li);
    });
}

// ----------------------------------------------------
// 4. 게시글 삭제 함수 (DELETE)
// ----------------------------------------------------
async function deletePost() {
    const postId = getPostId();
    if (!confirm("정말 이 글을 삭제하시겠습니까?")) return;

    try {
        const response = await fetch(`${API_URL}/${postId}`, { method: 'DELETE' });

        if (response.ok) {
            alert("삭제되었습니다.");
            location.href = 'board.html';
        } else {
            const msg = await response.text();
            alert("삭제 실패: " + msg);
        }
    } catch (error) {
        console.error(error);
        alert("서버 통신 오류가 발생했습니다.");
    }
}

// ----------------------------------------------------
// 5. 댓글 제출 이벤트 핸들러 (CREATE)
// ----------------------------------------------------
const commentForm = document.getElementById('comment-form');
const commentContentInput = document.getElementById('comment-content');

if (commentForm) { // 폼 요소가 있을 때만 리스너 등록 (안전장치)
    commentForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const postId = getCurrentPostId();
        const content = commentContentInput.value.trim();

        if (!content) {
            alert("댓글 내용을 입력해주세요.");
            return;
        }

        try {
            const response = await fetch(`${API_URL}/${postId}/comments`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                // DTO 필드명에 맞춰 'Content'를 대문자로 전송
                body: JSON.stringify({ Content: content })
            });

            const result = await response.json();

            if (response.status === 201) {
                alert("댓글이 성공적으로 등록되었습니다.");
                commentContentInput.value = ''; // 입력창 비우기
                fetchPostDetail(); // 목록 새로고침
            } else {
                alert(`댓글 등록 실패: ${result.message || '서버 오류'}`);
            }
        } catch (error) {
            console.error('댓글 API 통신 오류:', error);
            alert('서버 통신 오류가 발생했습니다.');
        }
    });
}


// ----------------------------------------------------
// 6. 전역 함수 등록 및 초기 실행
// ----------------------------------------------------
function getCurrentPostId() {
    return getPostId();
}

// 전역 스코프에 함수 등록 (HTML onclick 사용 시 필요)
window.deletePost = deletePost;
window.getCurrentPostId = getCurrentPostId;

// 페이지 로드 시 상세 정보 로드 시작
fetchPostDetail();