using CampusVisitorApi.DTOs;

namespace CampusVisitorApi.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task RegisterAsync(RegisterRequest request);
    Task<UserInfo> GetUserInfoAsync(int userId);
    Task ChangePasswordAsync(int userId, ChangePasswordRequest request);
}
