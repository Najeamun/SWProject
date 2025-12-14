// main-script.js
// API 엔드포인트 정의
const API_POSTS = 'http://localhost:5501/api/posts';
const API_GAMES = 'http://localhost:5501/api/boardgames';
const API_MEETINGS = 'http://localhost:5501/api/meetings';

// ----------------------------------------------------
// 페이지 로드 시 초기화 및 데이터 로딩 (중복 제거)
// ----------------------------------------------------
document.addEventListener("DOMContentLoaded", () => {
    // 1. 로그인 상태 확인 및 UI 업데이트
    updateLoginUI();

    // 2. 메인 페이지 대시보드 데이터 로드
    loadRecentPosts();
    loadRecommendedGames();
    loadRecentMeetings();

    // 3. 슬라이더 기능 초기화
    initSlider();
});

// ----------------------------------------------------
// 슬라이더 기능
// ----------------------------------------------------
function initSlider() {
    const sliderWrapper = document.querySelector('.slider-wrapper');
    const slides = document.querySelectorAll('.slide-item');
    const prevButton = document.querySelector('.prev');
    const nextButton = document.querySelector('.next');

    // 슬라이더 요소가 없으면 실행 중단 (안전장치)
    if (!sliderWrapper || slides.length === 0) return;

    let currentIndex = 0;
    const totalSlides = slides.length;
    // 초기 슬라이드 너비를 DOM이 완전히 로드된 후 계산
    let slideWidth = slides[0].clientWidth;

    // 슬라이드 위치 업데이트: X축 이동
    function updateSlider() {
        sliderWrapper.style.transform = `translateX(${-currentIndex * slideWidth}px)`;
    }

    // 다음 슬라이드 이동
    function nextSlide() {
        currentIndex = (currentIndex + 1) % totalSlides;
        updateSlider();
    }

    // 이전 슬라이드 이동
    function prevSlide() {
        // 인덱스가 음수가 되지 않도록 처리
        currentIndex = (currentIndex - 1 + totalSlides) % totalSlides;
        updateSlider();
    }

    // 네비게이션 버튼 이벤트 연결
    nextButton.addEventListener('click', nextSlide);
    prevButton.addEventListener('click', prevSlide);

    // 자동 슬라이드 (7초마다 전환)
    setInterval(nextSlide, 6000);

    // 윈도우 크기 변경 시 슬라이드 너비 재계산 및 위치 재조정
    window.addEventListener('resize', () => {
        slideWidth = slides[0].clientWidth;
        updateSlider();
    });

    // 초기 상태 표시
    updateSlider();
}

// ----------------------------------------------------
// 1. 로그인 UI 업데이트 (헤더 우측 상단)
// ----------------------------------------------------
function updateLoginUI() {
    const nickname = localStorage.getItem('nickname');
    const profileArea = document.getElementById('user-profile');

    // user-profile 영역이 있고 닉네임이 있을 경우 (로그인 상태)
    if (profileArea && nickname) {
        profileArea.innerHTML = `
            <a href="../../pages/auth/profile.html" style="color: white; margin-right:10px; font-weight: bold; text-decoration: none;">
                👋 ${nickname}님
            </a>
            <a href="#" id="logout-link-sub" style="color:#ffcccc; text-decoration:underline; font-size:0.9em; cursor: pointer;">로그아웃</a>
        `;
        document.getElementById('logout-link-sub').addEventListener('click', logout);
    }
}

// ----------------------------------------------------
// 2. 로그아웃 기능 (토큰 및 사용자 정보 삭제)
// ----------------------------------------------------
function logout() {
    if (confirm("로그아웃 하시겠습니까?")) {
        localStorage.clear(); // 모든 저장 정보 삭제
        alert("로그아웃 되었습니다.");
        // 로그아웃 후 로그인 페이지이동 
        window.location.href = '../../pages/auth/login.html';
    }
}

// ----------------------------------------------------
// 3. 최신 게시글 로드 (메인 대시보드)
// ----------------------------------------------------
async function loadRecentPosts() {
    const list = document.getElementById('main-board-list');
    if (!list) return;

    try {
        const res = await fetch(API_POSTS);
        if (!res.ok) throw new Error();
        const posts = await res.json();

        list.innerHTML = '';

        if (!posts || posts.length === 0) {
            list.innerHTML = '<li style="justify-content:center; color:#999;">등록된 글이 없습니다.</li>';
            return;
        }

        posts.slice(0, 5).forEach(post => {
            const date = new Date(post.createdAt).toLocaleDateString('ko-KR', { month: '2-digit', day: '2-digit' });
            const pid = post.postId || post.id; // DB DTO 필드명 통일성 방어

            list.innerHTML += `
                <li>
                    <a href="pages/board/detail.html?id=${pid}">📄 ${post.title || post.Title}</a>
                    <span class="summary-date">${date}</span>
                </li>`;
        });
    } catch (e) {
        if (list) list.innerHTML = '<li style="color:red;">게시글 로드 실패</li>';
    }
}

// ----------------------------------------------------
// 4. 추천 게임 로드 (메인 대시보드)
// ----------------------------------------------------
async function loadRecommendedGames() {
    const container = document.getElementById('main-game-list');
    if (!container) return;

    try {
        const res = await fetch(API_GAMES);
        if (!res.ok) throw new Error();
        const games = await res.json();

        container.innerHTML = '';

        // 평점(difficultyRating) 기준 내림차순 정렬 및 상위 3개 표시
        games.sort((a, b) => (b.difficultyRating || 0) - (a.difficultyRating || 0));

        games.slice(0, 3).forEach(game => {
            const img = game.imageUrl || '';
            const rating = (game.difficultyRating || 0).toFixed(1);
            const id = game.id || game.gameId;

            container.innerHTML += `
                <div class="rec-game-item" onclick="location.href='pages/board/game-detail.html?id=${id}'">
                    <img src="${img}" class="rec-game-img" onerror="this.style.background='#ccc'">
                    <div style="flex:1;">
                        <div style="font-weight:bold; font-size:1.05em;">${game.nameKo}</div>
                        <div style="font-size:0.85em; color:#666; margin-top:3px;">
                            <span style="color:#ff9800;">★ ${rating}</span> | ${game.category || '기타'}
                        </div>
                    </div>
                </div>`;
        });
    } catch (e) {
        if (container) container.innerHTML = '<p style="text-align:center; color:red;">추천 게임 로드 실패</p>';
    }
}

// ----------------------------------------------------
// 5. 모집 중인 모임 로드 (메인 대시보드)
// ----------------------------------------------------
async function loadRecentMeetings() {
    const list = document.getElementById('main-meeting-list');
    if (!list) return;

    try {
        const res = await fetch(API_MEETINGS);
        if (!res.ok) throw new Error();
        const meetings = await res.json();

        list.innerHTML = '';

        if (!meetings || meetings.length === 0) {
            list.innerHTML = '<li style="justify-content:center; color:#999;">모집 중인 모임이 없습니다.</li>';
            return;
        }

        meetings.slice(0, 5).forEach(m => {
            const dateObj = new Date(m.meetingTime);
            // 날짜 형식 지정: MM.DD HH:mm
            const dateStr = `${dateObj.getMonth() + 1}.${dateObj.getDate()} ${dateObj.getHours()}:${String(dateObj.getMinutes()).padStart(2, '0')}`;

            list.innerHTML += `
                <li>
                    <a href="pages/board/meetings.html">
                        <span style="font-weight:bold; color:#007bff; margin-right:5px;">[${m.location}]</span> ${m.title}
                    </a>
                    <span class="summary-date">${m.currentParticipants}/${m.maxParticipants}명</span>
                </li>`;
        });
    } catch (e) {
        if (list) list.innerHTML = '<li style="color:red;">모임 로드 실패</li>';
    }
}

// 🚨 로그인/로그아웃 처리를 위한 전역 함수 등록
window.logout = logout;