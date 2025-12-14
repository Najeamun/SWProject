// game-detail-script.js

const API_GAMES_URL = 'http://localhost:5501/api/boardgames'; // 🚨 포트 확인
const loadingElement = document.getElementById('loading');
const contentElement = document.getElementById('game-content');
const reviewListElement = document.getElementById('review-list');

// URL에서 게임 ID 추출
function getGameId() {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get('id');
}

// 상세 정보 로드
async function fetchGameDetail() {
    const gameId = getGameId();
    if (!gameId) {
        alert("잘못된 접근입니다.");
        location.href = 'games.html';
        return;
    }

    try {
        const response = await fetch(`${API_GAMES_URL}/${gameId}`);

        if (!response.ok) throw new Error("게임 정보를 찾을 수 없습니다.");

        const game = await response.json();
        renderGameDetail(game);

    } catch (error) {
        console.error(error);
        loadingElement.textContent = "데이터를 불러오는데 실패했습니다.";
    }
}

// 화면 렌더링
function renderGameDetail(game) {
    // 텍스트 정보 매핑
    document.getElementById('game-name-ko').textContent = game.nameKo;
    document.getElementById('game-name-en').textContent = game.nameEn;
    document.getElementById('game-rating').textContent = (game.difficultyRating || 0).toFixed(1);
    document.getElementById('game-category').textContent = game.category;
    document.getElementById('game-players').textContent = `${game.minPlayers} ~ ${game.maxPlayers}`;
    document.getElementById('game-time').textContent = game.playTimeMin;
    document.getElementById('game-difficulty').textContent = game.difficultyRating;
    document.getElementById('game-designer').textContent = game.designer;

    // 설명 (데이터가 없으면 카테고리 설명으로 대체)
    document.getElementById('game-description').textContent =
        game.description || game.categoryDescription || "설명 정보가 없습니다.";

    // 외부 링크
    const linkBtn = document.getElementById('game-external-link');
    if (game.externalLink) {
        linkBtn.href = game.externalLink;
    } else {
        linkBtn.style.display = 'none';
    }

    // 이미지 처리
    const imgContainer = document.getElementById('game-img-container');
    if (game.imageUrl) {
        imgContainer.innerHTML = `<img src="${game.imageUrl}" alt="${game.nameKo}">`;
    } else {
        imgContainer.innerHTML = `<div style="width:100%; height:100%; display:flex; align-items:center; justify-content:center; font-size:50px;">🎲</div>`;
    }

    // 리뷰 목록 렌더링
    renderReviews(game.reviews);

    // 로딩 화면 숨기고 콘텐츠 표시
    loadingElement.style.display = 'none';
    contentElement.style.display = 'block';
}

function renderReviews(reviews) {
    reviewListElement.innerHTML = '';

    if (!reviews || reviews.length === 0) {
        reviewListElement.innerHTML = '<li>아직 작성된 리뷰가 없습니다. 첫 번째 리뷰를 남겨보세요!</li>';
        return;
    }

    reviews.forEach(review => {
        const li = document.createElement('li');
        li.className = 'review-item';
        li.innerHTML = `
            <div class="review-header">
                <strong>${review.authorUsername}</strong>
                <span>${new Date(review.createdAt).toLocaleDateString()}</span>
            </div>
            <div>
                <span class="review-rating">★ ${review.rating}</span> 
                ${review.content}
            </div>
        `;
        reviewListElement.appendChild(li);
    });
}
// 리뷰 작성 버튼 이벤트 연결
async function submitReview() {
    const gameId = getGameId();
    const rating = document.getElementById('review-rating').value;
    const content = document.getElementById('review-content').value;

    const reviewData = {
        rating: parseInt(rating),
        content: content
    };

    try {
        const response = await fetch(`${API_GAMES_URL}/${gameId}/reviews`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(reviewData)
        });

        if (response.status === 201) {
            alert("리뷰가 등록되었습니다!");
            // 페이지 새로고침하여 리뷰 반영
            location.reload();
        } else {
            alert("리뷰 등록에 실패했습니다.");
        }
    } catch (error) {
        console.error('리뷰 작성 오류:', error);
        alert("서버 통신 오류가 발생했습니다.");
    }
}

// HTML의 버튼 onclick 속성을 이 함수로 변경해야 합니다.
// game-detail.html 파일에서 버튼을 찾아 다음과 같이 수정하거나,
// 아래 코드로 이벤트 리스너를 추가하세요.

// 기존 버튼 찾기 및 이벤트 리스너 추가
const submitBtn = document.querySelector('.review-form button');
if (submitBtn) {
    submitBtn.onclick = submitReview; // 기존 alert() 제거 및 함수 연결
}

// 실행
fetchGameDetail();