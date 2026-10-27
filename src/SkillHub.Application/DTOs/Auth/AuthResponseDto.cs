namespace SkillHub.Application.DTOs.Auth;

public record AuthResponseDto(
    string Token,
    string Email,
    string FullName,
    string Role
);    