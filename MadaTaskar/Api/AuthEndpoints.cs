using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MadaTaskar.Data;

namespace MadaTaskar.Api;

public static class AuthEndpoints
{
    static string HashPassword(string password) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password))).ToLower();

    public static void MapAuthApi(this WebApplication app)
    {
        app.MapPost("/api/auth/login", async (HttpContext ctx, IDbContextFactory<AppDbContext> factory, LoginRequest req) =>
        {
            using var db = await factory.CreateDbContextAsync();
            var hash = HashPassword(req.Password);
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == req.Username && u.PasswordHash == hash);
            if (user is null)
                return Results.Unauthorized();

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new("DisplayName", user.DisplayName),
                new("IsAdmin", user.IsAdmin.ToString())
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            return Results.Ok(new { user.DisplayName, user.IsAdmin });
        });

        app.MapPost("/api/auth/logout", async (HttpContext ctx) =>
        {
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Ok();
        });
    }
}

public record LoginRequest(string Username, string Password);
