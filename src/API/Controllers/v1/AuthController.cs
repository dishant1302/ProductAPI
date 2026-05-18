using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductAPI.Application.Common;
using ProductAPI.Application.DTOs.Auth;
using ProductAPI.Application.Interfaces;

namespace ProductAPI.API.Controllers.v1;

/// <summary>Authentication endpoints for registration, login, and token management.</summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[ApiController]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>Register a new user account.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequestDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registration attempt for {Email}", dto.Email);
        var result = await _authService.RegisterAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Register),
            ApiResponse<AuthResponseDto>.SuccessResult(result, "User registered successfully."));
    }

    /// <summary>Login and receive an access token and refresh token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Login attempt for {Email}", dto.Email);
        var result = await _authService.LoginAsync(dto, cancellationToken);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResult(result));
    }

    /// <summary>Exchange a valid refresh token for a new access token.</summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequestDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshTokenAsync(dto.RefreshToken, cancellationToken);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResult(result));
    }

    /// <summary>Revoke a refresh token (logout).</summary>
    [HttpPost("revoke-token")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeToken(
        [FromBody] RefreshTokenRequestDto dto,
        CancellationToken cancellationToken)
    {
        await _authService.RevokeTokenAsync(dto.RefreshToken, cancellationToken);
        return NoContent();
    }
}