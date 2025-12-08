// DTO:t (Data Transfer Objects) käyttöliittymän ja API:n väliseen tiedonsiirtoon.
// Nämä luokat kuvaavat JSON-rakenteita, joita Minimal API sitoo automaattisesti.
public record RegisterRequest(string Username, string Password);

/// <summary>
/// Lähetys JSON-bodylle: { "username": "kayttaja", "password": "salasana" }
/// Käytetään rekisteröitymisessä.
/// </summary>
public record LoginRequest(string Username, string Password);
