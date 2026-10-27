using SkillHub.Application.DTOs.Auth;

namespace SkillHub.Application.Common.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(string userId);
    Task<UserDto?> GetByEmailAsync(string email);
}