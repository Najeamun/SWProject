using Microsoft.EntityFrameworkCore; // 필수!
using SWProject.ApiService.Data;    // 필수!
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Cors;
using System.Text.Json.Serialization;
using SWProject.ApiService.Services;

var builder = WebApplication.CreateBuilder(args);

// =================================================================
// [1] 서비스 등록 (Services Registration)
// =================================================================

// 1. CORS 정책 설정
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

// 2. DB 연결 설정 (🚨 이 부분이 추가되었습니다!)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var serverVersion = new MySqlServerVersion(new Version(8, 0, 31));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, serverVersion, mysqlOptions =>
    {
        mysqlOptions.SchemaBehavior(MySqlSchemaBehavior.Ignore);
    })
);

// 3. 컨트롤러 서비스 등록 (+ 순환 참조 방지 옵션)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// 4. 사용자 서비스 등록 (DI)
// DB 컨텍스트가 위에서 등록되었으므로 이제 에러가 나지 않습니다.
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<BoardGameService>();
builder.Services.AddScoped<MeetingService>();

// =================================================================
// [2] 미들웨어 구성 (Middleware Pipeline)
// =================================================================

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // app.UseSwagger();
    // app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var gameService = services.GetRequiredService<BoardGameService>();

        // 🚨 이 함수 이름을 정확히 써야 합니다! (API + 번역)
        gameService.SeedGamesFromApiAsync().Wait();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"데이터 초기화 중 오류 발생: {ex.Message}");
    }
}

app.Run();