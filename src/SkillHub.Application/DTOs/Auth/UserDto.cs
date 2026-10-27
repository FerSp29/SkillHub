namespace SkillHub.Application.DTOs.Auth;

public record UserDto(
    string Id,
    string FullName,
    string Email,
    string? Specialty,
    bool IsEmployee
);    