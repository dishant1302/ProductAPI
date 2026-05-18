using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ProductAPI.Application.Common;
using ProductAPI.Application.DTOs.Auth;
using ProductAPI.Infrastructure.Data;

namespace API.Tests.Controllers;

public class AuthControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public AuthControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        // Ensure DB is created
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsCreated()
    {
        // Arrange
        var dto = new RegisterRequestDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = $"john_{Guid.NewGuid()}@test.com",
            Password = "Password1"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
        body!.Success.Should().BeTrue();
        body.Data!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var dto = new LoginRequestDto
        {
            Email = "nonexistent@test.com",
            Password = "WrongPassword1"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var dto = new RegisterRequestDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "not-an-email",
            Password = "Password1"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}