// games-script.js

const API_GAMES_URL = 'http://localhost:5501/api/boardgames';
const gameGrid = document.getElementById('game-grid');
const searchInput = document.getElementById('search-input');
const searchBtn = document.getElementById('search-btn');

async function fetchBoardGames() {
    // gameGrid가 없는 페이지(메인 등)에서는 실행 안 함
    if (!gameGrid) return;

    try {
        const response = await fetch(API_GAMES_URL);

        if (!response.ok) {
            throw new Error('게임 정보를 불러오는데 실패했습니다.');
        }

        const games = await response.json();
        renderGames(games);

    } catch (error) {
        console.error('게임 로드 오류:', error);
        gameGrid.innerHTML = '<p style="text-align:center; width:100%;">게임을 불러올 수 없습니다.</p>';
    }
}

function renderGames(games) {
    if (!gameGrid) return; // 안전장치
    gameGrid.innerHTML = '';

    if (!games || games.length === 0) {
        gameGrid.innerHTML = '<p style="text-align:center; width:100%;">등록된 보드게임이 없습니다.</p>';
        return;
    }

    games.forEach(game => {
        const card = document.createElement('div');
        card.className = 'game-card';

        // 🚨 주의: DB에서 받아온 ID 변수명이 'id'인지 'gameId'인지 확인 필요 (보통 id)
        // 여기서는 안전하게 game.id || game.gameId 로 처리
        const id = game.id || game.gameId;

        card.onclick = () => location.href = `game-detail.html?id=${id}`;

        const imageHtml = game.imageUrl
            ? `<img src="${game.imageUrl}" alt="${game.nameKo}" style="width:100%; height:200px; object-fit:cover;">`
            : `<div class="game-img-placeholder">🎲</div>`;

        // 🚨 [에러 수정 부분] 평점이 없으면 0으로 처리
        const rating = (game.difficultyRating || 0).toFixed(1);

        card.innerHTML = `
            ${imageHtml}
            <div class="game-info">
                <h3 class="game-title">${game.nameKo}</h3>
                <div class="game-meta">
                    <span>${game.category || '기타'}</span>
                    <span class="rating-badge">★ ${rating}</span>
                </div>
            </div>
        `;

        gameGrid.appendChild(card);
    });
}

// 검색 함수
async function searchGames() {
    const query = document.getElementById('search-input').value.trim();
    const category = document.getElementById('category-select').value; // ✅ 선택된 카테고리 가져오기

    // 쿼리 스트링 만들기
    let url = `${API_GAMES_URL}/search?category=${encodeURIComponent(category)}`;

    if (query) {
        url += `&query=${encodeURIComponent(query)}`;
    }

    try {
        const response = await fetch(url);
        if (!response.ok) throw new Error('검색 실패');

        const games = await response.json();
        renderGames(games);

    } catch (error) {
        console.error('검색 오류:', error);
        alert('검색 중 오류가 발생했습니다.');
    }
}

// 이벤트 리스너 등록 (요소가 있을 때만)
if (searchBtn) searchBtn.addEventListener('click', searchGames);
if (searchInput) {
    searchInput.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') searchGames();
    });
}

// 페이지 로드 시 실행
fetchBoardGames();