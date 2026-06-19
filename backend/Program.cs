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
