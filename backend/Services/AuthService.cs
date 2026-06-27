using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using CampusVisitorApi.Data;
using CampusVisitorApi.DTOs;
using CampusVisitorApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CampusVisitorApi.Services;

public class AuthService : IAuthService
{
    private readonly CampusVisitorDbContext _db;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;

    public AuthService(CampusVisitorDbContext db, IMapper mapper, IConfiguration config)
    {
        _db = db;
        _mapper = mapper;
        _config = config;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Phone == request.Phone);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("手机号或密码错误");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("账户已被禁用");

        // 检查是否在黑名单中
        var blacklisted = await _db.Blacklists
            .AnyAsync(b => b.UserId == user.Id && b.IsActive && b.ExpiresAt > DateTime.UtcNow);
        if (blacklisted)
        {
            var bl = await _db.Blacklists
                .FirstAsync(b => b.UserId == user.Id && b.IsActive);
            throw new UnauthorizedAccessException(
                $"您已被列入黑名单，截止 {bl.ExpiresAt:yyyy-MM-dd} 前不可使用本系统。原因：{bl.Reason}");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var token = GenerateToken(user);

        return new LoginResponse
        {
            Token = token,
            User = _mapper.Map<UserInfo>(user),
        };
    }

    public async Task RegisterAsync(RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Phone == request.Phone))
            throw new InvalidOperationException("该手机号已注册");

        var user = new User
        {
            Name = request.Name,
            Phone = request.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "visitor",
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }

    public async Task<UserInfo> GetUserInfoAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("用户不存在");
        return _mapper.Map<UserInfo>(user);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("用户不存在");

        if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("原密码错误");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _db.SaveChangesAsync();
    }

    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "CampusVisitorDefaultKey2024!@#$%^&*()"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.MobilePhone, user.Phone),
            new Claim(ClaimTypes.Role, user.Role),
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "CampusVisitorApi",
            audience: _config["Jwt:Audience"] ?? "CampusVisitorApp",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
