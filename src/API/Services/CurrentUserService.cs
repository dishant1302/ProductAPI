using System.Security.Claims;
using ProductAPI.Application.Interfaces;

namespace ProductAPI.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? Email =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);

    public string? FullName =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
}