using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

namespace TietokantaAPI.Services;

// JwtService rakentaa JWT-tokenit sovelluksen käyttöön.
// Tokenit sisältävät vähintään seuraavat claims:
// - sub: käyttäjänimi
// - userId: käyttäjän tietokanta-id
// Konfiguraatio haetaan `IConfiguration`-palvelusta (appsettings.json):
// Jwt:Issuer, Jwt:Audience, Jwt:Key, Jwt:ExpiresMinutes.
// Varmista, että `Jwt:Key` on riittävän pitkä (väh. 32 tavua / 256 bittiä) HS256:lle.
public class JwtService
{
    private readonly IConfiguration _config;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly string _key;
    private readonly double _expiresMinutes;

    public JwtService(IConfiguration config)
    {
        _config = config;
        _issuer = _config["Jwt:Issuer"] ?? "VarastoAPI";
        _audience = _config["Jwt:Audience"] ?? "VarastoClient";
        _key = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT key puuttuu konfiguraatiosta");
        _expiresMinutes = double.TryParse(_config["Jwt:ExpiresMinutes"], out var m) ? m : 120;
    }

    // Generoi allekirjoitetun JWT:n, joka sisältää `userId`-claimin.
    // Palautettu token on valmis klientille (esim. login-endpointin palautus).
    public string GenerateToken(int userId, string username)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim("userId", userId.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expiresMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
