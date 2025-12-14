using Microsoft.EntityFrameworkCore;
using SWProject.ApiService.Data;
using SWProject.ApiService.Models;
using System.Net.Http.Json; // JSON 처리용
using System.Text.Json.Serialization; // JSON 속성 매핑용

namespace SWProject.ApiService.Services
{
    // ==========================================
    // 0. 네이버 API 응답용 DTO 클래스
    // ==========================================
    public class NaverImageSearchResponse
    {
        [JsonPropertyName("items")]
        public List<NaverImageItem> Items { get; set; }
    }

    public class NaverImageItem
    {
        [JsonPropertyName("link")]
        public string Link { get; set; } // 원본 이미지 링크

        [JsonPropertyName("thumbnail")]
        public string Thumbnail { get; set; } // 썸네일 링크
    }

    // ==========================================
    // BoardGameService 메인 클래스
    // ==========================================
    public class BoardGameService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;

        // 🚨 [필수] 네이버 개발자 센터에서 발급받은 키를 여기에 입력하세요!
        private readonly string _naverClientId = "I9_eZAmnPaKH80qCQvdh";
        private readonly string _naverClientSecret = "yyn_7pINRx";

        public BoardGameService(AppDbContext context)
        {
            _context = context;
            _httpClient = new HttpClient();
        }

        // ==========================================
        // 1. 기본 조회 및 리뷰 기능 (Controller에서 사용)
        // ==========================================
        public async Task<List<BoardGame>> GetBoardGamesAsync() => await _context.BoardGames.ToListAsync();
        public async Task<BoardGame?> GetBoardGameByIdAsync(int id) => await _context.BoardGames.FindAsync(id);

        public async Task<List<BoardGame>> SearchBoardGamesAsync(string query, string category)
        {
            var q = _context.BoardGames.AsQueryable();

            // 1. 카테고리 필터링 (전체가 아닐 경우)
            if (!string.IsNullOrEmpty(category) && category != "전체")
            {
                q = q.Where(g => g.Category == category);
            }

            // 2. 검색어 필터링 (검색어가 있을 경우)
            if (!string.IsNullOrEmpty(query))
            {
                q = q.Where(g => g.NameKo.Contains(query) || g.NameEn.Contains(query));
            }

            return await q.ToListAsync();
        }

        public async Task<BoardGame?> GetBoardGameDetailAsync(int id)
        {
            return await _context.BoardGames
                .Include(g => g.Reviews).ThenInclude(r => r.User)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<GameReview> AddReviewAsync(int gameId, int userId, int rating, string content)
        {
            var review = new GameReview { BoardGameId = gameId, UserId = userId, Rating = rating, Content = content, CreatedAt = DateTime.UtcNow };
            _context.GameReviews.Add(review);
            await _context.SaveChangesAsync();

            // 평균 평점 업데이트 로직
            var game = await _context.BoardGames.Include(g => g.Reviews).FirstOrDefaultAsync(g => g.Id == gameId);
            if (game != null && game.Reviews.Any())
            {
                game.DifficultyRating = (decimal)game.Reviews.Average(r => r.Rating);
                await _context.SaveChangesAsync();
            }
            return review;
        }

        // ==========================================
        // 2. 대규모 데이터 주입 및 업데이트 (핵심!)
        // ==========================================
        public async Task SeedGamesFromApiAsync()
        {
            Console.WriteLine("🚀 [데이터 정비] 카테고리를 직관적으로 변경하고 이미지 수집을 시작합니다...");

            // ✅ 카테고리 단순화: 전략, 파티, 심리, 추리, 가족
            var targetGames = new List<BoardGame>
            {
                // === 1. 파티 & 심리 (다같이 웃고 떠들거나 속이는 게임) ===
                new BoardGame { Id = 101, NameKo="할리갈리", NameEn="Halli Galli", Category="파티", MinPlayers=2, MaxPlayers=6, PlayTimeMin=10, DifficultyRating=1.0m, Designer="Haim Shafir", CategoryDescription="과일이 5개가 되면 종을 쳐라! 전설의 순발력 게임" },
                new BoardGame { Id = 102, NameKo="뱅", NameEn="Bang!", Category="심리", MinPlayers=4, MaxPlayers=7, PlayTimeMin=40, DifficultyRating=1.6m, Designer="Emiliano Sciarra", CategoryDescription="보안관과 무법자의 숨막히는 서부 총격전 (블러핑)" },
                new BoardGame { Id = 103, NameKo="딕싯", NameEn="Dixit", Category="파티", MinPlayers=3, MaxPlayers=6, PlayTimeMin=30, DifficultyRating=1.2m, Designer="Jean-Louis Roubira", CategoryDescription="아름다운 일러스트를 보고 이야기꾼의 설명을 맞히는 감성 게임" },
                new BoardGame { Id = 104, NameKo="러브레터", NameEn="Love Letter", Category="심리", MinPlayers=2, MaxPlayers=4, PlayTimeMin=20, DifficultyRating=1.1m, Designer="Seiji Kanai", CategoryDescription="공주에게 마음을 전하기 위한 16장 카드의 눈치 싸움" },
                new BoardGame { Id = 105, NameKo="코드네임", NameEn="Codenames", Category="파티", MinPlayers=4, MaxPlayers=8, PlayTimeMin=15, DifficultyRating=1.3m, Designer="Vlaada Chvátil", CategoryDescription="단어 연상을 통해 우리 편 스파이를 찾아내는 팀 대항전" },
                new BoardGame { Id = 106, NameKo="더 마인드", NameEn="The Mind", Category="파티", MinPlayers=2, MaxPlayers=4, PlayTimeMin=15, DifficultyRating=1.1m, Designer="Wolfgang Warsch", CategoryDescription="말없이 서로의 눈빛만으로 숫자를 오름차순으로 내는 텔레파시 게임" },
                new BoardGame { Id = 107, NameKo="스컬", NameEn="Skull", Category="심리", MinPlayers=3, MaxPlayers=6, PlayTimeMin=30, DifficultyRating=1.2m, Designer="Hervé Marly", CategoryDescription="꽃인가 해골인가? 단순하지만 강렬한 거짓말 게임" },
                new BoardGame { Id = 108, NameKo="라스베가스", NameEn="Las Vegas", Category="파티", MinPlayers=2, MaxPlayers=5, PlayTimeMin=30, DifficultyRating=1.1m, Designer="Rüdiger Dorn", CategoryDescription="주사위를 굴려 카지노의 상금을 획득하는 운과 전략의 조화" },
                new BoardGame { Id = 109, NameKo="바퀴벌레 포커", NameEn="Cockroach Poker", Category="심리", MinPlayers=2, MaxPlayers=6, PlayTimeMin=20, DifficultyRating=1.1m, Designer="Jacques Zeimet", CategoryDescription="혐오스러운 동물 카드를 상대에게 떠넘기는 뻔뻔한 거짓말 게임" },
                new BoardGame { Id = 110, NameKo="달무티", NameEn="The Great Dalmuti", Category="파티", MinPlayers=4, MaxPlayers=8, PlayTimeMin=60, DifficultyRating=1.2m, Designer="Richard Garfield", CategoryDescription="신분 계급이 실시간으로 바뀌는 인생 역전 카드 게임" },
                new BoardGame { Id = 111, NameKo="텔레스트레이션", NameEn="Telestrations", Category="파티", MinPlayers=4, MaxPlayers=8, PlayTimeMin=30, DifficultyRating=1.0m, Designer="USAopoly", CategoryDescription="그림으로 전달하는 엉망진창 전달 게임 (스케치북 게임)" },
                new BoardGame { Id = 112, NameKo="아발론", NameEn="The Resistance: Avalon", Category="심리", MinPlayers=5, MaxPlayers=10, PlayTimeMin=30, DifficultyRating=1.7m, Designer="Don Eskridge", CategoryDescription="아서 왕의 신하들 사이에 숨어든 악의 하수인을 찾아내는 마피아 게임" },
                new BoardGame { Id = 113, NameKo="젠가", NameEn="Jenga", Category="파티", MinPlayers=1, MaxPlayers=8, PlayTimeMin=20, DifficultyRating=1.1m, Designer="Leslie Scott", CategoryDescription="나무 블록 탑을 무너뜨리지 않고 하나씩 빼내는 긴장감" },

                // === 2. 가족 & 입문 (누구나 쉽게 배우는 게임) ===
                new BoardGame { Id = 201, NameKo="스플렌더", NameEn="Splendor", Category="가족", MinPlayers=2, MaxPlayers=4, PlayTimeMin=30, DifficultyRating=1.8m, Designer="Marc André", CategoryDescription="보석 칩을 모아 귀족의 후원을 받는 국민 전략 게임" },
                new BoardGame { Id = 202, NameKo="티켓 투 라이드", NameEn="Ticket to Ride", Category="가족", MinPlayers=2, MaxPlayers=5, PlayTimeMin=45, DifficultyRating=1.8m, Designer="Alan R. Moon", CategoryDescription="기차 카드를 모아 북미 대륙의 도시들을 연결하세요" },
                new BoardGame { Id = 203, NameKo="루미큐브", NameEn="Rummikub", Category="가족", MinPlayers=2, MaxPlayers=4, PlayTimeMin=20, DifficultyRating=1.7m, Designer="Ephraim Hertzano", CategoryDescription="숫자 타일을 규칙에 맞게 조합하여 털어내는 두뇌 게임" },
                new BoardGame { Id = 204, NameKo="카르카손", NameEn="Carcassonne", Category="가족", MinPlayers=2, MaxPlayers=5, PlayTimeMin=35, DifficultyRating=1.9m, Designer="Klaus-Jürgen Wrede", CategoryDescription="타일을 이어 붙여 중세의 성, 도로, 들판을 완성하는 게임" },
                new BoardGame { Id = 205, NameKo="아줄", NameEn="Azul", Category="가족", MinPlayers=2, MaxPlayers=4, PlayTimeMin=30, DifficultyRating=1.7m, Designer="Michael Kiesling", CategoryDescription="알록달록한 타일을 가져와 왕궁의 벽을 아름답게 장식하세요" },
                new BoardGame { Id = 206, NameKo="센추리: 향신료의 길", NameEn="Century: Spice Road", Category="가족", MinPlayers=2, MaxPlayers=5, PlayTimeMin=45, DifficultyRating=1.8m, Designer="Emerson Matsuuchi", CategoryDescription="대상인이 되어 향신료를 교역하고 부를 축적하세요" },
                new BoardGame { Id = 207, NameKo="킹도미노", NameEn="Kingdomino", Category="가족", MinPlayers=2, MaxPlayers=4, PlayTimeMin=15, DifficultyRating=1.2m, Designer="Bruno Cathala", CategoryDescription="도미노 타일을 연결하여 나만의 작은 왕국을 건설하세요" },
                new BoardGame { Id = 208, NameKo="캐스캐디아", NameEn="Cascadia", Category="가족", MinPlayers=1, MaxPlayers=4, PlayTimeMin=45, DifficultyRating=1.8m, Designer="Randy Flynn", CategoryDescription="북미의 야생 동물과 서식지를 조화롭게 배치하는 힐링 게임" },
                new BoardGame { Id = 209, NameKo="타케노코", NameEn="Takenoko", Category="가족", MinPlayers=2, MaxPlayers=4, PlayTimeMin=45, DifficultyRating=1.9m, Designer="Antoine Bauza", CategoryDescription="황제의 정원에서 대나무를 키우고 판다를 돌보는 귀여운 게임" },
                new BoardGame { Id = 210, NameKo="모노폴리", NameEn="Monopoly", Category="가족", MinPlayers=2, MaxPlayers=8, PlayTimeMin=60, DifficultyRating=1.6m, Designer="Charles Darrow", CategoryDescription="부동산 거래를 통해 파산하지 않고 끝까지 살아남는 고전 게임" },
                new BoardGame { Id = 211, NameKo="부루마불", NameEn="Blue Marble", Category="가족", MinPlayers=2, MaxPlayers=4, PlayTimeMin=60, DifficultyRating=1.5m, Designer="씨앗사", CategoryDescription="세계 여행을 하며 도시를 건설하는 한국의 국민 보드게임" },

                // === 3. 전략 (머리를 쓰는 깊이 있는 게임) ===
                new BoardGame { Id = 301, NameKo="카탄", NameEn="Catan", Category="전략", MinPlayers=3, MaxPlayers=4, PlayTimeMin=60, DifficultyRating=2.3m, Designer="Klaus Teuber", CategoryDescription="자원을 모아 마을과 도로를 건설하고 거래하는 협상 게임의 바이블" },
                new BoardGame { Id = 302, NameKo="7 원더스", NameEn="7 Wonders", Category="전략", MinPlayers=2, MaxPlayers=7, PlayTimeMin=30, DifficultyRating=2.3m, Designer="Antoine Bauza", CategoryDescription="문명을 발전시키고 불가사의를 건설하여 역사에 이름을 남기세요" },
                new BoardGame { Id = 303, NameKo="도미니언", NameEn="Dominion", Category="전략", MinPlayers=2, MaxPlayers=4, PlayTimeMin=30, DifficultyRating=2.3m, Designer="Donald X. Vaccarino", CategoryDescription="자신만의 카드 덱을 강화하여 영지를 넓히는 덱 빌딩 게임" },
                new BoardGame { Id = 304, NameKo="스톤 에이지", NameEn="Stone Age", Category="전략", MinPlayers=2, MaxPlayers=4, PlayTimeMin=60, DifficultyRating=2.5m, Designer="Bernd Brunnhofer", CategoryDescription="부족민을 일터에 보내 자원을 모으고 문명을 발전시키는 석기시대 게임" },
                new BoardGame { Id = 305, NameKo="테라포밍 마스", NameEn="Terraforming Mars", Category="전략", MinPlayers=1, MaxPlayers=5, PlayTimeMin=120, DifficultyRating=3.2m, Designer="Jacob Fryxelius", CategoryDescription="화성을 인류가 살 수 있는 행성으로 개척하는 기업 경쟁 프로젝트" },
                new BoardGame { Id = 306, NameKo="아그리콜라", NameEn="Agricola", Category="전략", MinPlayers=1, MaxPlayers=4, PlayTimeMin=90, DifficultyRating=3.6m, Designer="Uwe Rosenberg", CategoryDescription="17세기 농부가 되어 밭을 갈고 가축을 기르며 가족을 부양하세요" },
                new BoardGame { Id = 307, NameKo="푸에르토 리코", NameEn="Puerto Rico", Category="전략", MinPlayers=3, MaxPlayers=5, PlayTimeMin=90, DifficultyRating=3.3m, Designer="Andreas Seyfarth", CategoryDescription="농작물을 생산하고 선적하여 점수를 얻는 식민지 경영 게임의 걸작" },
                new BoardGame { Id = 308, NameKo="사이쓰", NameEn="Scythe", Category="전략", MinPlayers=1, MaxPlayers=5, PlayTimeMin=115, DifficultyRating=3.4m, Designer="Jamey Stegmaier", CategoryDescription="대체 역사 1920년대를 배경으로 한 메카닉과 농업의 전략 게임" },
                new BoardGame { Id = 309, NameKo="글룸헤이븐", NameEn="Gloomhaven", Category="전략", MinPlayers=1, MaxPlayers=4, PlayTimeMin=120, DifficultyRating=3.8m, Designer="Isaac Childres", CategoryDescription="방대한 세계관과 시나리오를 자랑하는 최고의 판타지 모험" },
                new BoardGame { Id = 310, NameKo="아크 노바", NameEn="Ark Nova", Category="전략", MinPlayers=1, MaxPlayers=4, PlayTimeMin=90, DifficultyRating=3.7m, Designer="Mathias Wigge", CategoryDescription="최고의 동물원을 설계하고 운영하여 매력 점수와 보호 점수를 얻으세요" },
                new BoardGame { Id = 311, NameKo="듄: 임페리움", NameEn="Dune: Imperium", Category="전략", MinPlayers=1, MaxPlayers=4, PlayTimeMin=60, DifficultyRating=3.0m, Designer="Paul Dennen", CategoryDescription="영화 듄의 세계관에서 펼쳐지는 덱 빌딩과 일꾼 놓기의 조화" },
                new BoardGame { Id = 312, NameKo="버건디의 성", NameEn="The Castles of Burgundy", Category="전략", MinPlayers=2, MaxPlayers=4, PlayTimeMin=90, DifficultyRating=3.0m, Designer="Stefan Feld", CategoryDescription="주사위를 사용하여 영지를 번영시키는 전략 게임의 클래식" },
                new BoardGame { Id = 313, NameKo="루트", NameEn="Root", Category="전략", MinPlayers=2, MaxPlayers=4, PlayTimeMin=90, DifficultyRating=3.7m, Designer="Cole Wehrle", CategoryDescription="숲의 지배권을 두고 벌어지는 귀여운 동물들의 전쟁" },
                new BoardGame { Id = 314, NameKo="오딘을 위하여", NameEn="A Feast for Odin", Category="전략", MinPlayers=1, MaxPlayers=4, PlayTimeMin=120, DifficultyRating=3.8m, Designer="Uwe Rosenberg", CategoryDescription="바이킹의 삶을 체험하며 사냥, 약탈, 무역을 통해 번영하세요" },
                new BoardGame { Id = 315, NameKo="그레이트 웨스턴 트레일", NameEn="Great Western Trail", Category="전략", MinPlayers=2, MaxPlayers=4, PlayTimeMin=75, DifficultyRating=3.7m, Designer="Alexander Pfister", CategoryDescription="소 떼를 이끌고 캔자스 시티까지 이동하며 서부 개척 시대를 체험하세요" },
                new BoardGame { Id = 316, NameKo="에버델", NameEn="Everdell", Category="전략", MinPlayers=1, MaxPlayers=4, PlayTimeMin=40, DifficultyRating=2.8m, Designer="James A. Wilson", CategoryDescription="아름다운 숲속 마을을 건설하는 일꾼 놓기 및 엔진 빌딩 게임" },
                new BoardGame { Id = 317, NameKo="브라스: 버밍엄", NameEn="Brass: Birmingham", Category="전략", MinPlayers=2, MaxPlayers=4, PlayTimeMin=60, DifficultyRating=3.9m, Designer="Gavan Brown", CategoryDescription="산업 혁명 시대의 기업가가 되어 운하와 철도를 연결하세요" },
                new BoardGame { Id = 318, NameKo="콩코르디아", NameEn="Concordia", Category="전략", MinPlayers=2, MaxPlayers=5, PlayTimeMin=100, DifficultyRating=3.0m, Designer="Mac Gerdts", CategoryDescription="로마 제국의 무역망을 확장하며 고대 지중해의 패권을 잡으세요" },
                new BoardGame { Id = 319, NameKo="시타델", NameEn="Citadels", Category="전략", MinPlayers=2, MaxPlayers=8, PlayTimeMin=60, DifficultyRating=2.4m, Designer="Bruno Faidutti", CategoryDescription="매 라운드 다른 캐릭터를 선택하여 건물을 짓고 상대를 견제하세요" },
                new BoardGame { Id = 320, NameKo="윙스팬", NameEn="Wingspan", Category="전략", MinPlayers=1, MaxPlayers=5, PlayTimeMin=40, DifficultyRating=2.4m, Designer="Elizabeth Hargrave", CategoryDescription="새들을 불러모아 최고의 조류 보호구역을 만드는 게임" },

                // === 4. 추리 & 2인 전용 (머리 싸움) ===
                new BoardGame { Id = 401, NameKo="다빈치 코드", NameEn="Coda", Category="추리", MinPlayers=2, MaxPlayers=4, PlayTimeMin=15, DifficultyRating=1.2m, Designer="Eiji Wakasugi", CategoryDescription="상대의 숨겨진 숫자 타일을 논리적으로 추리해 맞히세요" },
                new BoardGame { Id = 402, NameKo="클루", NameEn="Clue", Category="추리", MinPlayers=3, MaxPlayers=6, PlayTimeMin=45, DifficultyRating=1.8m, Designer="Anthony E. Pratt", CategoryDescription="저택에서 일어난 살인 사건의 범인, 도구, 장소를 찾아내세요" },
                new BoardGame { Id = 403, NameKo="스플렌더 대결", NameEn="Splendor Duel", Category="전략", MinPlayers=2, MaxPlayers=2, PlayTimeMin=30, DifficultyRating=2.0m, Designer="Marc André", CategoryDescription="2인 전용으로 더욱 치열하게 재탄생한 스플렌더" },
                new BoardGame { Id = 404, NameKo="패치워크", NameEn="Patchwork", Category="전략", MinPlayers=2, MaxPlayers=2, PlayTimeMin=30, DifficultyRating=1.6m, Designer="Uwe Rosenberg", CategoryDescription="천 조각을 잘 기워 최고의 이불을 만드는 2인용 퍼즐 게임" },
                new BoardGame { Id = 405, NameKo="산토리니", NameEn="Santorini", Category="전략", MinPlayers=2, MaxPlayers=2, PlayTimeMin=20, DifficultyRating=1.7m, Designer="Gord!", CategoryDescription="그리스 신들의 능력을 사용하여 3층 탑을 먼저 쌓으세요" },
                new BoardGame { Id = 406, NameKo="자이푸르", NameEn="Jaipur", Category="전략", MinPlayers=2, MaxPlayers=2, PlayTimeMin=30, DifficultyRating=1.5m, Designer="Sébastien Pauchon", CategoryDescription="인도 최고의 상인이 되기 위한 2인 전용 거래 카드 게임" },
                new BoardGame { Id = 407, NameKo="로스트 시티", NameEn="Lost Cities", Category="전략", MinPlayers=2, MaxPlayers=2, PlayTimeMin=30, DifficultyRating=1.5m, Designer="Reiner Knizia", CategoryDescription="오지로 탐험을 떠나 명성을 얻는 2인 전용 숫자 카드 게임" },
                new BoardGame { Id = 408, NameKo="사그라다", NameEn="Sagrada", Category="전략", MinPlayers=1, MaxPlayers=4, PlayTimeMin=30, DifficultyRating=1.9m, Designer="Adrian Adamescu", CategoryDescription="아름다운 주사위를 배치하여 성당의 스테인드글라스를 완성하세요" },
                new BoardGame { Id = 409, NameKo="스피릿 아일랜드", NameEn="Spirit Island", Category="전략", MinPlayers=1, MaxPlayers=4, PlayTimeMin=90, DifficultyRating=4.0m, Designer="R. Eric Reuss", CategoryDescription="정령이 되어 침략자들로부터 섬을 지켜내는 고난이도 협력 게임" },
                new BoardGame { Id = 410, NameKo="팬데믹", NameEn="Pandemic", Category="전략", MinPlayers=2, MaxPlayers=4, PlayTimeMin=45, DifficultyRating=2.4m, Designer="Matt Leacock", CategoryDescription="질병 치료 전문가 팀이 되어 전 세계를 위협하는 전염병을 막으세요" },
                new BoardGame { Id = 411, NameKo="더 크루", NameEn="The Crew", Category="전략", MinPlayers=3, MaxPlayers=5, PlayTimeMin=20, DifficultyRating=2.0m, Designer="Thomas Sing", CategoryDescription="우주를 배경으로 미션을 수행하는 협력 트릭테이킹 게임" }
            };

            foreach (var game in targetGames)
            {
                try
                {
                    // 네이버 이미지 검색 요청
                    // "보드게임 + 이름" 조합으로 검색어 최적화
                    string query = $"보드게임 {game.NameKo}";
                    string url = $"https://openapi.naver.com/v1/search/image?query={query}&display=1&sort=sim";

                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Add("X-Naver-Client-Id", _naverClientId);
                    request.Headers.Add("X-Naver-Client-Secret", _naverClientSecret);

                    var response = await _httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<NaverImageSearchResponse>();

                        if (result != null && result.Items.Count > 0)
                        {
                            var existingGame = await _context.BoardGames.FirstOrDefaultAsync(g => g.NameKo == game.NameKo);

                            if (existingGame != null)
                            {
                                // ✅ 카테고리도 함께 업데이트 (테마 -> 전략 등으로 변경됨)
                                existingGame.Category = game.Category;
                                existingGame.CategoryDescription = game.CategoryDescription;

                                // 이미지가 없는 경우에만 업데이트하거나, 강제로 덮어쓰기
                                existingGame.ImageUrl = result.Items[0].Link;

                                Console.WriteLine($"✅ [갱신] {game.NameKo} ({game.Category})");
                            }
                            else
                            {
                                game.ImageUrl = result.Items[0].Link;
                                game.ExternalLink = "https://search.naver.com/search.naver?query=" + query;
                                _context.BoardGames.Add(game);
                                Console.WriteLine($"✅ [신규] {game.NameKo}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ 이미지 검색 실패 ({response.StatusCode}) - {game.NameKo}");
                    }

                    // API 호출 제한 고려 (0.05초 대기)
                    await Task.Delay(50);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ 에러 발생: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            Console.WriteLine("🎉 [완료] 카테고리 정비 및 데이터 업데이트 완료!");
        }
    }
}