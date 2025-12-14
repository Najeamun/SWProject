// board-script.js
// 자유 게시판 목록 조회 및 필터링 기능 담당

const API_URL = 'http://localhost:5501/api/posts'; // API 기본 경로

document.addEventListener('DOMContentLoaded', () => {
    // 초기 로드
    fetchPosts();

    // 카테고리 필터 이벤트 리스너를 DOMContentLoaded 내부에서 처리
    const categoryFilter = document.getElementById('board-category-filter');
    if (categoryFilter) {
        categoryFilter.addEventListener('change', filterPosts);
    }
});

// 카테고리 변경 시 호출되는 함수
function filterPosts() {
    const category = document.getElementById('board-category-filter').value;
    fetchPosts(category);
}

// ----------------------------------------------------
// 게시글 목록 조회 및 필터링 (READ)
// ----------------------------------------------------
async function fetchPosts(category = "전체") {
    const url = `${API_URL}?category=${encodeURIComponent(category)}`;

    try {
        const response = await fetch(url);

        if (!response.ok) throw new Error('게시글 목록을 불러오는데 실패했습니다.');

        const posts = await response.json();
        renderPosts(posts);

    } catch (error) {
        console.error("데이터 로드 에러:", error);
        document.getElementById('posts-table-body').innerHTML =
            `<tr><td colspan="6" style="text-align:center; padding: 20px;">데이터를 불러올 수 없습니다. 서버 상태를 확인하세요.</td></tr>`;
    }
}

// ----------------------------------------------------
// 게시글 목록 테이블 렌더링
// ----------------------------------------------------
function renderPosts(posts) {
    const tableBody = document.getElementById('posts-table-body');
    if (!tableBody) return; // 안전장치

    tableBody.innerHTML = '';

    if (!posts || posts.length === 0) {
        tableBody.innerHTML = `<tr><td colspan="6" style="text-align:center; padding: 20px;">작성된 글이 없습니다.</td></tr>`;
        return;
    }

    posts.forEach((post, index) => {
        const row = tableBody.insertRow();

        // 1. 번호 (최신 글이 높은 번호)
        row.insertCell().textContent = posts.length - index;

        // 2. 분류 (카테고리)
        const catCell = row.insertCell();
        catCell.textContent = post.category || post.Category || '잡담';
        catCell.style.color = '#E67E22';
        catCell.style.fontWeight = 'bold';
        catCell.style.textAlign = 'center';

        // 3. 제목 (클릭 링크)
        const titleCell = row.insertCell();
        titleCell.className = 'title-cell';

        // 게시글 ID와 댓글 수 추출 (DTO 필드명 방어)
        const realId = post.postId || post.PostId || post.id;
        const commentCount = post.commentCount || post.CommentCount || 0;

        titleCell.innerHTML = `<a href="detail.html?id=${realId}">
                                 ${post.title || post.Title} 
                                 <span style="color:#999; font-size:0.8em; margin-left:5px;">[${commentCount}]</span>
                                </a>`;

        // 4. 작성자
        row.insertCell().textContent = post.authorUsername || post.AuthorUsername || '익명';

        // 5. 작성일
        const createdDate = post.createdAt || post.CreatedAt;
        const dateStr = createdDate ? new Date(createdDate).toLocaleDateString() : '-';
        row.insertCell().textContent = dateStr;

        // 6. 조회수
        row.insertCell().textContent = post.viewCount || post.ViewCount || 0;
    });
}
