using System.Text;
using System.Text.Json.Serialization;
using CampusVisitorApi.Data;
using CampusVisitorApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ========== Services ==========

// Database
builder.Services.AddDbContext<CampusVisitorDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "CampusVisitorDefaultKey2024!@#$%^&*()";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "CampusVisitorApi",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "CampusVisitorApp",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
    });

builder.Services.AddAuthorization();

// AutoMapper
builder.Services.AddAutoMapper(typeof(CampusVisitorApi.Mappings.MappingProfile));

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IEntryExitService, EntryExitService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// Controllers + JSON 配置（防止循环引用）
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ========== Middleware Pipeline ==========

var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// 自动创建数据库（如果不存在）
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CampusVisitorDbContext>();
    db.Database.EnsureCreated();

    // 创建状态变更触发器（EnsureCreated 不会执行 SQL 脚本中的触发器）
    try
    {
        db.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.triggers WHERE parent_id = OBJECT_ID('Reservations') AND name = 'trg_Reservations_StatusChange')
            BEGIN
                EXEC('CREATE TRIGGER [dbo].[trg_Reservations_StatusChange]
                ON [dbo].[Reservations] AFTER UPDATE AS BEGIN SET NOCOUNT ON
                IF UPDATE([Status])
                BEGIN
                    INSERT INTO [dbo].[ReservationStatusLog]
                        ([ReservationId], [FromStatus], [ToStatus], [OperatorId], [Remark])
                    SELECT i.[Id], d.[Status], i.[Status], i.[ReviewerId],
                        CASE i.[Status]
                            WHEN ''approved'' THEN N''审核通过''
                            WHEN ''rejected'' THEN N''审核拒绝''
                            WHEN ''cancelled'' THEN N''用户取消预约''
                            WHEN ''checked_in'' THEN N''已入校核验''
                            WHEN ''checked_out'' THEN N''已离校登记''
                            WHEN ''no_show'' THEN N''系统标记爽约''
                            ELSE N''状态变更'' END
                    FROM INSERTED i INNER JOIN DELETED d ON i.[Id] = d.[Id]
                END END')
            END");
    }
    catch { /* 触发器已存在或创建失败 */ }

    // 开发环境：修正密码哈希为 BCrypt 格式
    try
    {
        var users = db.Users.Where(u => !u.PasswordHash.StartsWith("$2")).ToList();
        foreach (var user in users)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456");
        }
        if (users.Count > 0) db.SaveChanges();
    }
    catch { /* 忽略种子错误 */ }
}

app.Run();
