using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Feezbow.Domain.Entities;
using Feezbow.Infrastructure.Options;

namespace Feezbow.Infrastructure.Services;

public class JwtTokenService(IOptions<JwtOptions> _options, UserManager<User> userManager)
    : IJwtTokenService
{
    // Neconfirmovani korisnik sme da koristi sistem ovoliko sati od registracije,
    // posle čega ga "EmailConfirmed" policy blokira dok ne potvrdi mejl.
    private const int EmailConfirmationGraceHours = 24;

    private  JwtOptions Options => _options.Value;
    public async Task<string> GenerateTokenAsync(User user, CancellationToken cancellationToken = default)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("email_confirmed", user.EmailConfirmed.ToString().ToLower()),
        };

        // Za nepotvrđene: rok do kog grace prozor važi (unix sekunde, UTC).
        if (!user.EmailConfirmed)
        {
            var requestedAt = user.EmailConfirmationRequestedAt ?? user.CreatedAt;
            var deadlineUtc = DateTime.SpecifyKind(requestedAt, DateTimeKind.Utc)
                .AddHours(EmailConfirmationGraceHours);
            claims.Add(new Claim(
                "confirm_deadline",
                ((DateTimeOffset)deadlineUtc).ToUnixTimeSeconds().ToString()));
        }

        var roles = await userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes( Options.Key ?? string.Empty));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(Options.ExpiryMinutes);

        var token = new JwtSecurityToken(
            Options.Issuer,
            Options.Audience,
            claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public DateTime GetRefreshTokenExpiry()
        => DateTime.UtcNow.AddDays(Options.RefreshTokenExpiryDays);
}