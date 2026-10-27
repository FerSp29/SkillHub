using Microsoft.AspNetCore.Identity;
using SkillHub.Application.Common.Interfaces;
using SkillHub.Application.DTOs.Auth;
using SkillHub.Application.Services;
using SkillHub.Infrastructure.Identity;

namespace SkillHub.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;

    public AuthService(UserManager<ApplicationUser> userManager, IJwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new ArgumentException("A user with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new ArgumentException($"Failed to create user: {errors}");
        }

        // All external registrations are assigned the 'Participant' role by default
        await _userManager.AddToRoleAsync(user, "Participant");

        var token = _jwtService.GenerateToken(user.Id, user.Email, user.FullName, new[] { "Participant" });
        return new AuthResponseDto(token, user.Email, user.FullName, "Participant");
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? "Participant";
        var token = _jwtService.GenerateToken(user.Id, user.Email!, user.FullName, roles);

        return new AuthResponseDto(token, user.Email!, user.FullName, primaryRole);
    }
}