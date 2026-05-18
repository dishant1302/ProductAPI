using Microsoft.EntityFrameworkCore; 
using Asp.Versioning;
using FluentValidation.AspNetCore;
using ProductAPI.API.Extensions;
using ProductAPI.API.Filters;
using ProductAPI.API.Middleware;
using ProductAPI.API.Services;
using ProductAPI.Application;
using ProductAPI.Application.Interfaces;
using ProductAPI.Infrastructure;
using ProductAPI.Infrastructure.Data;
using Serilog;

// ── Bootstrap logger (before host is built) ──────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ───────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((context, services, cfg) => cfg
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day));

    // ── Application & Infrastructure ──────────────────────────────────────────
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // ── Controllers ───────────────────────────────────────────────────────────
    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ValidationFilter>();
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Suppress default 400 behavior so our ValidationFilter handles it
        options.SuppressModelStateInvalidFilter = true;
    });

    // ── FluentValidation ─────────────────────────────────────────────────────
    builder.Services.AddFluentValidationAutoValidation();

    // ── API Versioning ────────────────────────────────────────────────────────
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new HeaderApiVersionReader("X-API-Version"),
            new QueryStringApiVersionReader("api-version"));
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    // ── Swagger ───────────────────────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerWithJwt();

    // ── CORS ──────────────────────────────────────────────────────────────────
    builder.Services.AddCors(options =>
        options.AddPolicy("AllowAll", policy =>
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

    // ── CurrentUserService ────────────────────────────────────────────────────
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

    // ── Response Compression ─────────────────────────────────────────────────
    builder.Services.AddResponseCompression(options => options.EnableForHttps = true);

    // ── Security Headers ──────────────────────────────────────────────────────
    builder.Services.AddHsts(options =>
    {
        options.Preload = true;
        options.IncludeSubDomains = true;
        options.MaxAge = TimeSpan.FromDays(365);
    });

    // ─────────────────────────────────────────────────────────────────────────
    var app = builder.Build();

    // ── Middleware pipeline (ORDER MATTERS) ───────────────────────────────────
    app.UseMiddleware<ErrorHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProductAPI v1");
            c.RoutePrefix = string.Empty; // Swagger at root URL
        });
    }
    else
    {
        app.UseHsts();
    }

    app.UseSerilogRequestLogging();
    app.UseResponseCompression();
    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // ── Auto-run migrations on startup ────────────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
        Log.Information("Database migration applied successfully.");
    }

    Log.Information("ProductAPI starting up on {Environment}", app.Environment.EnvironmentName);
    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}

// Expose Program class for integration tests
public partial class Program { }