namespace SkillHub.Application.DTOs.Auth;

public record RegisterRequestDto(
    string FullName,
    string Email,
    string Password
);    