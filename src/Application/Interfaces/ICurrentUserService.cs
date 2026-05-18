namespace ProductAPI.Application.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
    string? FullName { get; }
}