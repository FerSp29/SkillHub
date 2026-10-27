using System.Security.Claims;

namespace SkillHub.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(string userId, string email, string fullName, IEnumerable<string> roles);
}