using Microsoft.AspNetCore.Identity;
using SkillHub.Application.Common.Interfaces;
using SkillHub.Application.DTOs.Auth;
using SkillHub.Infrastructure.Identity;

namespace SkillHub.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserDto?> GetByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        var isEmployee = await _userManager.IsInRoleAsync(user, "Employee");
        return new UserDto(user.Id, user.FullName, user.Email!, user.Specialty, isEmployee);
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return null;

        var isEmployee = await _userManager.IsInRoleAsync(user, "Employee");
        return new UserDto(user.Id, user.FullName, user.Email!, user.Specialty, isEmployee);
    }
}