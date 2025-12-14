// write-script.js
// 게시글 작성(POST) 요청 처리

const API_URL = 'http://localhost:5501/api/posts'; // API 기본 경로

const postForm = document.getElementById('post-form');
const titleInput = document.getElementById('title');
const contentInput = document.getElementById('content');

postForm.addEventListener('submit', async (e) => {
    e.preventDefault();

    // 1. 폼 데이터 수집
    const postData = {
        // 말머리(카테고리) 값을 가져옴
        category: document.getElementById('category').value,
        title: titleInput.value.trim(),
        content: contentInput.value.trim()
    };

    if (!postData.title || !postData.content) {
        alert("제목과 내용을 모두 입력해주세요.");
        return;
    }

    try {
        // 2. 게시글 작성 API 호출 (POST)
        const response = await fetch(API_URL, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(postData)
        });

        // 3. 응답 처리
        if (response.status === 201) {
            alert(`🎉 게시글이 성공적으로 작성되었습니다!`);
            location.href = 'board.html'; // 목록 페이지로 이동
        } else {
            const errorData = await response.json();
            console.error('작성 실패 서버 응답:', errorData);
            alert(`게시글 작성 실패: ${errorData.message || '서버 내부 오류'}`);
        }
    } catch (error) {
        console.error('API 통신 오류:', error);
        alert('서버에 연결할 수 없습니다. 서버 실행 상태를 확인하세요.');
    }
});